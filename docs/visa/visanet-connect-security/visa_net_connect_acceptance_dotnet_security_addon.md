# VisaNet Connect – Acceptance Security Add-on for .NET

## Purpose

This document extends the payment-platform architecture handbook with the concrete VisaNet Connect – Acceptance security requirements for:

- Two-Way SSL / Mutual TLS (mTLS)
- Message Level Encryption (MLE)
- Certificate and key lifecycle
- Sandbox, Certification, and Production separation
- .NET integration patterns
- Monitoring, testing, and stage gates

## Official Visa links

- VisaNet Connect – Acceptance Authentication  
  https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-authentication

- Visa Two-Way SSL Guide  
  https://developer.visa.com/pages/working-with-visa-apis/two-way-ssl

- Visa Message Level Encryption  
  https://developer.visa.com/pages/encryption_guide

- VisaNet Connect – Acceptance API Reference  
  https://developer.visa.com/capabilities/visanet-connect-acceptance/reference

- Visa Developer Documentation  
  https://developer.visa.com/docs

## 1. Security architecture

mTLS and MLE are separate controls:

```text
Application
   |
   | JSON payload
   v
MLE encryption
   |
   | encrypted/JWE payload
   v
HttpClient
   |
   | mTLS
   v
VisaNet Connect – Acceptance
   |
   | mTLS
   v
HTTP response
   |
   | encrypted/JWE payload
   v
MLE decryption
   |
   v
Application
```

mTLS protects the transport channel and authenticates both endpoints.

MLE protects sensitive message content, including data such as PAN, PII, and account information.

## 2. MLE key model

Visa documents separate request and response encryption roles:

### Request
The client encrypts the request using the Visa server encryption public key. Visa decrypts using the corresponding private key.

### Response
Visa encrypts the response using the client encryption public key. The client decrypts using its private key.

Visa documentation describes MLE as JWE-based and documents AES-GCM for payload encryption and RSA-OAEP for key encryption. Verify the exact algorithms required by the active API/project before implementation.

## 3. Key-ID

Visa assigns a Key-ID for MLE key material. It must be external configuration because it changes during key rotation.

Example:

```json
{
  "Visa": {
    "Environment": "Sandbox",
    "BaseUrl": "https://sandbox.api.visa.com",
    "MtlsCertificateSecret": "visa-mtls-sandbox",
    "Mle": {
      "Enabled": true,
      "KeyId": "FROM-VISA-PROJECT",
      "VisaEncryptionCertificateSecret": "visa-mle-visa-public-cert",
      "ClientPrivateKeySecret": "visa-mle-client-private-key"
    }
  }
}
```

## 4. Recommended .NET structure

```text
Payments.Networks.Visa
|
+-- Configuration
|   +-- VisaOptions.cs
+-- Transport
|   +-- VisaHttpClientFactory.cs
+-- Encryption
|   +-- IVisaMessageEncryption.cs
|   +-- VisaMessageEncryption.cs
+-- Clients
|   +-- IVisaClient.cs
|   +-- VisaClient.cs
+-- Mapping
|   +-- VisaRequestMapper.cs
|   +-- VisaResponseMapper.cs
+-- Security
|   +-- VisaCertificateProvider.cs
|   +-- VisaKeyProvider.cs
+-- Observability
    +-- VisaTelemetry.cs
```

## 5. mTLS in .NET

```csharp
using System.Security.Cryptography.X509Certificates;

public interface IVisaCertificateProvider
{
    X509Certificate2 GetClientCertificate();
}
```

Example provider:

```csharp
public sealed class VisaCertificateProvider : IVisaCertificateProvider
{
    private readonly IConfiguration _configuration;

    public VisaCertificateProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public X509Certificate2 GetClientCertificate()
    {
        var path = _configuration["Visa:Mtls:PfxPath"]
            ?? throw new InvalidOperationException("Visa mTLS certificate path missing.");

        var password = _configuration["Visa:Mtls:PfxPassword"]
            ?? throw new InvalidOperationException("Visa mTLS certificate password missing.");

        return new X509Certificate2(
            path,
            password,
            X509KeyStorageFlags.EphemeralKeySet |
            X509KeyStorageFlags.MachineKeySet);
    }
}
```

For production, prefer a managed certificate store, vault, or HSM-backed solution rather than a local PFX.

## 6. HttpClientFactory integration

```csharp
builder.Services
    .AddHttpClient<IVisaClient, VisaClient>((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<VisaOptions>>().Value;

        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var certificateProvider =
            sp.GetRequiredService<IVisaCertificateProvider>();

        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual
        };

        handler.ClientCertificates.Add(
            certificateProvider.GetClientCertificate());

        return handler;
    });
```

## 7. Visa options

```csharp
public sealed class VisaOptions
{
    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 15;
    public VisaMleOptions Mle { get; init; } = new();
}

public sealed class VisaMleOptions
{
    public bool Enabled { get; init; }
    public string KeyId { get; init; } = string.Empty;
}
```

Register:

```csharp
builder.Services.Configure<VisaOptions>(
    builder.Configuration.GetSection("Visa"));
```

## 8. MLE abstraction

```csharp
public interface IVisaMessageEncryption
{
    string EncryptRequest(string plaintext);
    string DecryptResponse(string encryptedPayload);
}
```

Do not implement JWE logic inside the API client.

## 9. Request pipeline

```csharp
public sealed class VisaClient : IVisaClient
{
    private readonly HttpClient _httpClient;
    private readonly IVisaMessageEncryption _encryption;
    private readonly IOptions<VisaOptions> _options;

    public VisaClient(
        HttpClient httpClient,
        IVisaMessageEncryption encryption,
        IOptions<VisaOptions> options)
    {
        _httpClient = httpClient;
        _encryption = encryption;
        _options = options;
    }

    public async Task<string> PostAsync(
        string endpoint,
        object request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request);

        var payload = _options.Value.Mle.Enabled
            ? _encryption.EncryptRequest(json)
            : json;

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            endpoint);

        message.Content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        if (_options.Value.Mle.Enabled)
        {
            message.Headers.TryAddWithoutValidation(
                "keyId",
                _options.Value.Mle.KeyId);
        }

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var result = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Visa API returned HTTP {(int)response.StatusCode}.");
        }

        return _options.Value.Mle.Enabled
            ? _encryption.DecryptResponse(result)
            : result;
    }
}
```

The exact MLE header name and envelope must be verified against the active Visa API/project configuration.

## 10. MLE implementation strategy

Use a mature, security-reviewed JOSE/JWE library rather than hand-rolling cryptography.

The implementation should depend on:

```text
IVisaMessageEncryption
       |
       +-- JOSE/JWE implementation
              |
              +-- Visa public encryption certificate
              +-- Client private decryption key
```

Validate the chosen library and algorithms against Visa sandbox/certification before production.

## 11. Key provider

```csharp
public interface IVisaKeyProvider
{
    X509Certificate2 GetMtlsCertificate();
    RSA GetClientMlePrivateKey();
    X509Certificate2 GetVisaMleEncryptionCertificate();
    string GetActiveKeyId();
}
```

Store private keys in a managed secure store.

## 12. Certificate and key rotation

```text
Generate new key
      |
Create CSR
      |
Register with Visa
      |
Receive/activate certificate
      |
Deploy new Key-ID + key material
      |
Validate
      |
Switch active Key-ID
      |
Monitor
      |
Retire old key
```

The active Key-ID should be changeable without redeploying application code.

## 13. Environment separation

Use independent credentials and keys for:

```text
Sandbox
Certification
Production
```

Never reuse certificates, MLE private keys, Key-IDs, credentials, or endpoints across environments.

## 14. Observability

Recommended metrics:

```text
visa_mtls_handshake_failures_total
visa_mle_encryption_failures_total
visa_mle_decryption_failures_total
visa_mle_key_expiry_days
visa_mtls_certificate_expiry_days
visa_api_requests_total
visa_api_duration_seconds
visa_api_errors_total
```

Safe log fields:

- correlation ID
- environment
- operation
- HTTP status
- Key-ID
- certificate thumbprint
- latency
- trace ID

Never log:

- PAN
- CVV/CVC
- private keys
- raw encrypted/decrypted payloads
- track data
- cardholder PII

## 15. Error catalog

```text
VISA_TLS_CERTIFICATE_INVALID
VISA_TLS_HANDSHAKE_FAILED
VISA_TLS_CERTIFICATE_EXPIRED
VISA_MLE_KEY_NOT_FOUND
VISA_MLE_KEY_ID_INVALID
VISA_MLE_ENCRYPTION_FAILED
VISA_MLE_DECRYPTION_FAILED
VISA_MLE_RESPONSE_INVALID
VISA_AUTHENTICATION_FAILED
VISA_TIMEOUT
VISA_NETWORK_ERROR
VISA_API_CLIENT_ERROR
VISA_API_SERVER_ERROR
```

## 16. Retry policy

Do not blindly retry payment operations.

A timeout can mean either:

- the request never reached Visa; or
- Visa processed the request but the response was lost.

Use network-supported correlation/idempotency identifiers, operation-specific retry policies, and reconciliation for unknown outcomes.

## 17. Security tests

### mTLS
- valid certificate
- invalid certificate
- expired certificate
- wrong private key
- missing certificate
- bad trust chain

### MLE
- successful encryption/decryption
- wrong Key-ID
- wrong Visa public key
- wrong client private key
- corrupted JWE
- algorithm mismatch
- old/new key rotation

### Logging
Verify PAN, PII, keys, and raw payloads never appear.

## 18. Sandbox stage gate

- [ ] mTLS succeeds
- [ ] Visa server certificate is validated
- [ ] client certificate accepted
- [ ] MLE request encryption works
- [ ] MLE response decryption works
- [ ] Key-ID externalized
- [ ] private keys securely stored
- [ ] certificate expiry monitoring active
- [ ] encryption/decryption failures observable
- [ ] sensitive logging tests passed
- [ ] timeout/unknown outcome tested
- [ ] rotation procedure documented

Decision:

- [ ] GO
- [ ] CONDITIONAL GO
- [ ] NO GO

## 19. Certification stage gate

- [ ] separate certification mTLS certificate
- [ ] separate certification MLE key material
- [ ] certification Key-ID configured
- [ ] official certification scenarios passed
- [ ] request encryption verified
- [ ] response decryption verified
- [ ] error scenarios passed
- [ ] rotation tested
- [ ] monitoring validated
- [ ] evidence archived
- [ ] no critical/high security findings
- [ ] required sign-off obtained

## 20. Production stage gate

- [ ] production mTLS certificate installed
- [ ] production MLE keys installed
- [ ] production Key-ID configured
- [ ] secrets loaded from secure store
- [ ] rotation tested
- [ ] expiry alerts tested
- [ ] access restricted
- [ ] PCI review complete
- [ ] production logs verified
- [ ] SIEM alerts configured
- [ ] incident runbooks available
- [ ] rollback verified

## 21. Integration with the interchange architecture

```text
Payment / Virtual Card Service
           |
           v
Interchange Configuration
           |
           v
Visa Business Adapter
           |
           v
MLE Handler
           |
           v
mTLS HttpClient
           |
           v
VisaNet Connect – Acceptance
```

The interchange engine should not manage TLS certificates, JWE, or private encryption keys. Those responsibilities belong to the Visa integration/security layer.

## 22. Recommended handbook additions

```text
security/
  visa-network-security-profile.md
  visa-mtls.md
  visa-mle.md
  visa-key-management.md
  visa-certificate-rotation.md

dotnet/
  VisaOptions.cs
  VisaClient.cs
  VisaCertificateProvider.cs
  VisaMessageEncryption.cs
  VisaKeyProvider.cs

testing/
  visa-mtls-test-plan.md
  visa-mle-test-plan.md

runbooks/
  visa-certificate-expiry.md
  visa-mle-key-rotation.md
  visa-mtls-failure.md
  visa-mle-decryption-failure.md

stage-gates/
  visa-sandbox-security-gate.md
  visa-certification-security-gate.md
  visa-production-security-gate.md
```

## Final architectural requirement

For VisaNet Connect – Acceptance payment processing, treat the following as part of the base integration:

```text
mTLS
+
MLE
+
secure key lifecycle
+
PCI controls
+
auditable configuration
+
monitoring
```

Do not defer MLE and key-management design until production certification.
