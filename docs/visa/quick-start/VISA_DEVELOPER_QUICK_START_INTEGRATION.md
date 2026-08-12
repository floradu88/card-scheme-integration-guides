# Visa Developer Quick Start — Integration Add-on

> Extension to the existing generic payment-platform / dynamic-interchange implementation plan.
> Visa-specific material belongs in the network-adapter and onboarding layers, not in the generic interchange engine.

## Official source

Visa Developer Quick Start for Developers:
https://developer.visa.com/pages/working-with-visa-apis/visa-developer-quick-start-guide

Supporting official documentation:
- Working with Visa APIs: https://developer.visa.com/pages/working-with-visa-apis
- VisaNet Connect – Acceptance Authentication: https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-authentication
- Two-Way SSL: https://developer.visa.com/pages/working-with-visa-apis/two-way-ssl
- Message Level Encryption: https://developer.visa.com/pages/encryption_guide
- Outbound Callback Configuration: https://developer.visa.com/pages/working-with-visa-apis/outbound-configuration

## 1. Developer resources and assets

The implementation/onboarding workstream must account for the Visa Developer resources described by the Quick Start:

- Product and use-case catalog.
- Product documentation, platform documentation, standards and guidelines.
- Code Explorer / API Reference request and response examples.
- Sandbox and supplied test data.
- Product-specific sample code, including C#.
- Project Dashboard > Assets for implementation/integration guides.
- Authentication sample code / Hello World connectivity.
- JWE/JWS encryption/decryption material and samples.
- Common error-code documentation.
- Learning Hub and Visa support paths.

### Requirement
Create a `VisaSourceRegistry` that records which official artifact supports each implemented behavior and its last review date.

## 2. Account, access and project creation

Delivery checklist:

- [ ] Create organization-owned Visa Developer account.
- [ ] Determine whether the required product is public or restricted.
- [ ] If restricted, submit Request Access and track approval/resubmission.
- [ ] Confirm regional availability.
- [ ] Create project with the required products/APIs.
- [ ] Assign technical, security and operational owners.
- [ ] Review Onboarding Dashboard.
- [ ] Review Sandbox product/API status.
- [ ] Download credentials and test data securely.
- [ ] Inventory project Assets and implementation guides.

### Dashboard responsibilities

Onboarding Dashboard:
- add products/APIs;
- complete production onboarding steps.

Sandbox:
- products/APIs;
- credentials;
- test data;
- API/project status.

Assets:
- product documentation;
- integration assets;
- downloadable samples.

## 3. Authentication

Visa Developer APIs use the authentication method applicable to the selected API, including:

- Two-Way SSL / mutual authentication; or
- X-Pay-Token shared-secret authentication.

For VisaNet Connect – Acceptance, use the product-specific authentication requirements already captured in the security add-on.

### Architecture

```text
Domain Service
    |
Visa Adapter
    |
Authentication Strategy
    +---- mTLS
    |
    +---- X-Pay-Token (only where selected API requires it)
    |
Visa API
```

Do not allow business-domain code to construct authentication tokens or manipulate certificates.

## 4. Message Level Encryption

Some Visa products require MLE in addition to transport authentication.

Architecture:

```text
Domain request
   -> Visa mapping
   -> MLE/JWE encryption when required
   -> authenticated HTTP transport
   -> Visa
   -> authenticated response
   -> MLE/JWE decryption when required
   -> response mapping
```

MLE enablement, Key-ID, certificate/key material and algorithm selection must be environment-specific and externally configured.

## 5. Credential expiration and rotation

Visa credentials expire periodically.

The platform must inventory and monitor:

- mTLS certificates;
- MLE certificates/key material;
- X-Pay credentials where applicable.

### Operational requirements

- expiry monitoring;
- 90/60/30/14/7/1-day alerts;
- named owner;
- documented renewal;
- overlapping credential support where Visa permits it;
- no code deployment required merely to switch active credential references;
- post-rotation connectivity test;
- revocation procedure;
- audit evidence.

### Renewal workflow

```text
Expiry alert
 -> generate/obtain replacement
 -> update Visa project/onboarding track
 -> securely distribute replacement
 -> preload
 -> connectivity test
 -> activate
 -> monitor
 -> retire old credential
```

## 6. Visa Developer PKI

Visa uses X.509 PKI for Visa Developer endpoints.

Requirements:

- maintain trusted CA chain according to current Visa guidance;
- validate hostname and certificate chain;
- do not disable server certificate validation;
- manage trust-store updates as controlled changes;
- test trust changes before production;
- monitor TLS handshake failures.

## 7. Outbound callbacks

If the selected product sends Visa-originated callbacks, register an HTTPS endpoint for each applicable environment.

Visa's outbound callback documentation states the callback configuration is supported over Two-Way SSL.

### Setup workflow

1. Select Sandbox / Certification / Production as applicable.
2. Open Outbound Callback Configuration.
3. Add the callback domain/path.
4. Use HTTPS and a certificate issued by a Visa-accepted trusted CA.
5. Submit for review/approval.
6. Run ping tests after approval.
7. Support controlled deactivation.

### .NET receiver architecture

```text
Visa
  -> Internet/Edge
  -> mTLS termination/validation
  -> Callback API
  -> Schema validation
  -> Idempotency / deduplication
  -> Durable queue
  -> Callback processor
  -> Domain event
```

### Callback requirements

- [ ] HTTPS only.
- [ ] Correct client/server authentication controls.
- [ ] Schema validation.
- [ ] Request-size limits.
- [ ] Correlation.
- [ ] Idempotent processing.
- [ ] Duplicate-event handling.
- [ ] Durable acknowledgement strategy.
- [ ] Safe retry handling.
- [ ] No sensitive payload logging.
- [ ] Metrics and alerting.
- [ ] Replay/recovery procedure.

## 8. Going Live

Certification/production use requires completion of the Visa project Onboarding Dashboard steps.

Treat promotion as a formal stage gate:

```text
Development
 -> Sandbox
 -> Certification
 -> Production Validation
 -> Production
```

Do not infer production eligibility from successful sandbox calls.

## 9. Sandbox testing

Visa describes Sandbox as a functional test environment using mock/limited data.

### Endpoints documented by Quick Start

B2B:
`https://sandbox.api.visa.com/`

B2C:
`https://sandbox.webapi.visa.com/`

Use the endpoint applicable to the selected product.

### Project/API status

Track dashboard status and activation. Do not start automated integration testing until the selected API/project is ready.

### Sandbox exit checklist

- [ ] Credentials securely loaded.
- [ ] Authentication works.
- [ ] Hello World/connectivity succeeds where applicable.
- [ ] MLE works where required.
- [ ] Happy-path API calls pass.
- [ ] Negative/error cases pass.
- [ ] Timeout handling tested.
- [ ] Unknown-outcome handling tested.
- [ ] Callback ping test passes where applicable.
- [ ] Logs contain no prohibited cardholder/sensitive data.
- [ ] Metrics and traces available.
- [ ] Certificate/key rotation procedure documented.
- [ ] Production gaps documented.

## 10. Certification environment

Quick Start documents the certification base endpoint as:

`https://cert.api.visa.com/<URI>`

### mTLS credential handling

For the certification environment, plan for:

- CSR/private key generated for certification;
- certification username/public certificate obtained from the project;
- password/credential material obtained securely;
- common Visa certificates/trust chain;
- separate secrets from Sandbox and Production.

Connectivity can be validated against the certification Hello World endpoint when applicable.

### X-Pay-Token

Where an API uses X-Pay-Token:

- configure the certification API key/shared secret;
- implement the official signing/token algorithm;
- validate with the Visa-provided tools/samples;
- store secret material in the vault;
- never reuse production credentials.

### Certification gate

- [ ] Separate certification credentials.
- [ ] Certification endpoint configured.
- [ ] mTLS validated if applicable.
- [ ] X-Pay-Token validated if applicable.
- [ ] MLE validated if applicable.
- [ ] Product certification scenarios completed.
- [ ] Callback certification completed where applicable.
- [ ] Evidence archived.
- [ ] Security review complete.
- [ ] Operational monitoring complete.
- [ ] Visa/acquirer/processor approvals captured as applicable.

## 11. Production environment

Quick Start documents the production base endpoint as:

`https://api.visa.com/<URI>`

Production must use production-specific:

- private key / CSR;
- public certificate;
- username/password where required;
- common Visa certificates;
- X-Pay credentials where applicable;
- MLE keys and Key-ID where applicable;
- callback configuration.

### Production validation

- validate connectivity before traffic activation;
- run approved smoke tests;
- verify certificate chain;
- verify MLE;
- verify safe logging;
- verify alerts;
- verify callback endpoint if applicable;
- verify rollback;
- begin with controlled traffic/canary where architecture permits.

## 12. Visa Developer Center Playground and API Explorer

Use Visa-provided test tooling as diagnostic aids:

- API Explorer for endpoint/request/response exploration;
- VDC Playground for Visa Developer API testing/troubleshooting;
- Postman/SOAP UI flows where the official guide recommends them;
- product C# sample code as an implementation reference.

These tools complement, not replace, automated tests in the platform CI/CD pipeline.

## 13. .NET implementation blueprint

Recommended projects:

```text
Payments.Domain
Payments.Application

Payments.Networks.Visa
  /Authentication
  /Encryption
  /Callbacks
  /Clients
  /Configuration
  /Mapping
  /Telemetry

Payments.Networks.Visa.Tests
Payments.Networks.Visa.IntegrationTests
Payments.Networks.Visa.CertificationTests
```

### Environment options

```csharp
public sealed class VisaOptions
{
    public required Uri BaseUrl { get; init; }
    public VisaAuthenticationMode AuthenticationMode { get; init; }
    public bool MleEnabled { get; init; }
    public string? MleKeyId { get; init; }
    public string MtlsCertificateReference { get; init; } = "";
    public string? XPayApiKeyReference { get; init; }
    public string? XPaySecretReference { get; init; }
}
```

### Typed HttpClient

```csharp
services.AddHttpClient<IVisaClient, VisaClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<VisaOptions>>().Value;
    client.BaseAddress = options.BaseUrl;
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var options = sp.GetRequiredService<IOptions<VisaOptions>>().Value;
    var handler = new HttpClientHandler();

    if (options.AuthenticationMode == VisaAuthenticationMode.MutualTls)
    {
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ClientCertificates.Add(
            sp.GetRequiredService<IVisaCertificateProvider>()
              .GetClientCertificate());
    }

    return handler;
});
```

### Delegating-handler pipeline

Recommended:

```text
HttpClient
  -> CorrelationHandler
  -> TelemetryHandler
  -> XPayTokenHandler (only if applicable)
  -> MleHandler (only if applicable)
  -> SocketsHttpHandler / mTLS
```

Keep sensitive logging outside the raw body.

### Callback endpoint example

```csharp
app.MapPost("/callbacks/visa/{eventType}",
    async (
        string eventType,
        HttpRequest request,
        IVisaCallbackValidator validator,
        IVisaCallbackQueue queue,
        CancellationToken ct) =>
    {
        var callback = await validator.ValidateAsync(request, ct);

        await queue.EnqueueOnceAsync(
            callback.EventId,
            eventType,
            callback.SafePayload,
            ct);

        return Results.Ok();
    });
```

The concrete acknowledgement contract must follow the selected Visa API's callback specification.

## 14. Error handling

Classify errors into:

- authentication;
- authorization;
- TLS;
- MLE;
- validation;
- rate limiting;
- Visa/client 4xx;
- Visa/server 5xx;
- timeout;
- connection failure;
- unknown financial outcome;
- callback validation;
- duplicate callback.

Never blindly retry financial operations. Retry behavior must be operation-specific and must account for a request that may have been processed despite a lost response.

## 15. Monitoring

Metrics:

```text
visa_api_requests_total
visa_api_request_duration_seconds
visa_api_errors_total
visa_tls_handshake_failures_total
visa_mle_encrypt_failures_total
visa_mle_decrypt_failures_total
visa_xpay_auth_failures_total
visa_callback_received_total
visa_callback_duplicate_total
visa_callback_processing_failures_total
visa_credential_expiry_days
```

Alerts:

- production connectivity loss;
- sustained 401/403;
- TLS failures;
- MLE failures;
- callback backlog;
- callback validation failures;
- credentials within expiry thresholds;
- configuration mismatch between nodes.

## 16. CI/CD and secret handling

CI/CD must never embed production credentials in build artifacts.

Recommended:

```text
Build artifact
    +
Environment configuration
    +
Runtime secret references
    =
Running Visa adapter
```

Use managed identity/workload identity to retrieve secrets where possible.

## 17. Evidence to retain

For each environment:

- project/API configuration record;
- credential inventory without secret values;
- certificate thumbprints and validity dates;
- connectivity test evidence;
- MLE test evidence;
- callback ping-test evidence;
- certification results;
- production validation evidence;
- approval records;
- rotation evidence;
- incident evidence.

## 18. Master delivery checklist

### Discovery
- [ ] Correct Visa product identified.
- [ ] Regional availability confirmed.
- [ ] Restricted-access requirements identified.
- [ ] Sponsor/acquirer/processor dependencies known.

### Project
- [ ] Visa Developer account.
- [ ] Project created.
- [ ] APIs added.
- [ ] Assets reviewed.
- [ ] Owners assigned.

### Security
- [ ] Authentication method confirmed per API.
- [ ] mTLS implemented where required.
- [ ] X-Pay-Token implemented where required.
- [ ] MLE implemented where required.
- [ ] PKI/trust configured.
- [ ] Secrets vaulted.
- [ ] Rotation automated/documented.

### Sandbox
- [ ] Connectivity.
- [ ] Functional tests.
- [ ] Error tests.
- [ ] Security tests.
- [ ] Observability.
- [ ] Callbacks if applicable.

### Certification
- [ ] Separate credentials.
- [ ] Certification endpoint.
- [ ] Required scenarios.
- [ ] Evidence.
- [ ] Approval.

### Production
- [ ] Separate production credentials.
- [ ] Production endpoint.
- [ ] Production callback configuration.
- [ ] Smoke tests.
- [ ] Monitoring.
- [ ] On-call/runbooks.
- [ ] Rollback.
- [ ] Controlled go-live.

## 19. Relationship to Dynamic Interchange

Visa Developer onboarding/security is infrastructure and network integration.

Keep the boundary:

```text
Dynamic Interchange Domain
        |
        v
Network-neutral decision
        |
        v
Visa Adapter
        |
        +-- Visa request mapping
        +-- MLE
        +-- Authentication
        +-- Visa endpoint
        +-- Callback handling
        |
        v
Visa
```

This prevents Visa-specific credentials, PKI and onboarding concepts from contaminating the generic interchange configuration model.

## 20. Review rule

The Visa Developer site and project-specific Assets are authoritative for current implementation details. Before each certification or production promotion, re-check the active product documentation, credentials, API reference, onboarding dashboard, and restricted assets rather than relying solely on this handbook.
