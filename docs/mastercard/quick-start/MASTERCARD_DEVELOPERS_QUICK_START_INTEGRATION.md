# Mastercard Developers Quick Start — Integration Add-on

> Extension to the existing generic payment-platform / dynamic-interchange implementation plan.
> Mastercard-specific material belongs in the network-adapter and onboarding layers, not in the generic interchange engine.

## Official source

Mastercard Developers Quick Start for Developers:
https://developer.mastercard.com/platform/documentation/getting-started-with-mastercard-apis/quick-start-guide/

Supporting official documentation:
- Working with Mastercard APIs: https://developer.mastercard.com/pages/working-with-mastercard-apis
- Mastercard Developers APIs Authentication: https://developer.mastercard.com/platform/documentation/security-and-authentication/using-oauth-1a-to-access-mastercard-apis/
- mTLS / OAuth signing: https://developer.mastercard.com/platform/documentation/security-and-authentication/using-oauth-1a-to-access-mastercard-apis/
- Message Level Encryption: https://developer.mastercard.com/platform/documentation/security-and-authentication/securing-sensitive-data-using-payload-encryption/
- Outbound Callback Configuration: https://developer.mastercard.com/pages/working-with-mastercard-apis/outbound-configuration

## 1. Developer resources and assets

The implementation/onboarding workstream must account for the Mastercard Developers resources described by the Quick Start:

- Product and use-case catalog.
- Product documentation, platform documentation, standards and guidelines.
- Code Explorer / API Reference request and response examples.
- Sandbox and supplied test data.
- Product-specific sample code, including C#.
- Project Dashboard > Assets for implementation/integration guides.
- Authentication sample code / Hello World connectivity.
- JWE/JWS encryption/decryption material and samples.
- Common error-code documentation.
- Learning Hub and Mastercard support paths.

### Requirement
Create a `MastercardSourceRegistry` that records which official artifact supports each implemented behavior and its last review date.

## 2. Account, access and project creation

Delivery checklist:

- [ ] Create organization-owned Mastercard Developers account.
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

Mastercard Developers APIs use the authentication method applicable to the selected API, including:

- mTLS / OAuth signing / mutual authentication; or
- X-Pay-Token shared-secret authentication.

For Mastercard Developers APIs, use the product-specific authentication requirements already captured in the security add-on.

### Architecture

```text
Domain Service
    |
Mastercard Adapter
    |
Authentication Strategy
    +---- mTLS
    |
    +---- X-Pay-Token (only where selected API requires it)
    |
Mastercard API
```

Do not allow business-domain code to construct authentication tokens or manipulate certificates.

## 4. Message Level Encryption

Some Mastercard products require MLE in addition to transport authentication.

Architecture:

```text
Domain request
   -> Mastercard mapping
   -> MLE/JWE encryption when required
   -> authenticated HTTP transport
   -> Mastercard
   -> authenticated response
   -> MLE/JWE decryption when required
   -> response mapping
```

MLE enablement, Key-ID, certificate/key material and algorithm selection must be environment-specific and externally configured.

## 5. Credential expiration and rotation

Mastercard credentials expire periodically.

The platform must inventory and monitor:

- mTLS certificates;
- MLE certificates/key material;
- X-Pay credentials where applicable.

### Operational requirements

- expiry monitoring;
- 90/60/30/14/7/1-day alerts;
- named owner;
- documented renewal;
- overlapping credential support where Mastercard permits it;
- no code deployment required merely to switch active credential references;
- post-rotation connectivity test;
- revocation procedure;
- audit evidence.

### Renewal workflow

```text
Expiry alert
 -> generate/obtain replacement
 -> update Mastercard project/onboarding track
 -> securely distribute replacement
 -> preload
 -> connectivity test
 -> activate
 -> monitor
 -> retire old credential
```

## 6. Mastercard Developers PKI

Mastercard uses X.509 PKI for Mastercard Developers endpoints.

Requirements:

- maintain trusted CA chain according to current Mastercard guidance;
- validate hostname and certificate chain;
- do not disable server certificate validation;
- manage trust-store updates as controlled changes;
- test trust changes before production;
- monitor TLS handshake failures.

## 7. Outbound callbacks

If the selected product sends Mastercard-originated callbacks, register an HTTPS endpoint for each applicable environment.

Mastercard's outbound callback documentation states the callback configuration is supported over mTLS / OAuth signing.

### Setup workflow

1. Select Sandbox / Certification / Production as applicable.
2. Open Outbound Callback Configuration.
3. Add the callback domain/path.
4. Use HTTPS and a certificate issued by a Mastercard-accepted trusted CA.
5. Submit for review/approval.
6. Run ping tests after approval.
7. Support controlled deactivation.

### .NET receiver architecture

```text
Mastercard
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

Certification/production use requires completion of the Mastercard project Onboarding Dashboard steps.

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

Mastercard describes Sandbox as a functional test environment using mock/limited data.

### Endpoints documented by Quick Start

B2B:
`https://sandbox.api.mastercard.com/`

B2C:
`https://sandbox.webapi.mastercard.com/`

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

`https://cert.api.mastercard.com/<URI>`

### mTLS credential handling

For the certification environment, plan for:

- CSR/private key generated for certification;
- certification username/public certificate obtained from the project;
- password/credential material obtained securely;
- common Mastercard certificates/trust chain;
- separate secrets from Sandbox and Production.

Connectivity can be validated against the certification Hello World endpoint when applicable.

### X-Pay-Token

Where an API uses X-Pay-Token:

- configure the certification API key/shared secret;
- implement the official signing/token algorithm;
- validate with the Mastercard-provided tools/samples;
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
- [ ] Mastercard/acquirer/processor approvals captured as applicable.

## 11. Production environment

Quick Start documents the production base endpoint as:

`https://api.mastercard.com/<URI>`

Production must use production-specific:

- private key / CSR;
- public certificate;
- username/password where required;
- common Mastercard certificates;
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

## 12. Mastercard Developers Center Playground and API Explorer

Use Mastercard-provided test tooling as diagnostic aids:

- API Explorer for endpoint/request/response exploration;
- Mastercard Developers API client / Insomnia plugin for Mastercard Developers API testing/troubleshooting;
- Postman/SOAP UI flows where the official guide recommends them;
- product C# sample code as an implementation reference.

These tools complement, not replace, automated tests in the platform CI/CD pipeline.

## 13. .NET implementation blueprint

Recommended projects:

```text
Payments.Domain
Payments.Application

Payments.Networks.Mastercard
  /Authentication
  /Encryption
  /Callbacks
  /Clients
  /Configuration
  /Mapping
  /Telemetry

Payments.Networks.Mastercard.Tests
Payments.Networks.Mastercard.IntegrationTests
Payments.Networks.Mastercard.CertificationTests
```

### Environment options

```csharp
public sealed class MastercardOptions
{
    public required Uri BaseUrl { get; init; }
    public MastercardAuthenticationMode AuthenticationMode { get; init; }
    public bool MleEnabled { get; init; }
    public string? MleKeyId { get; init; }
    public string MtlsCertificateReference { get; init; } = "";
    public string? XPayApiKeyReference { get; init; }
    public string? XPaySecretReference { get; init; }
}
```

### Typed HttpClient

```csharp
services.AddHttpClient<IMastercardClient, MastercardClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<MastercardOptions>>().Value;
    client.BaseAddress = options.BaseUrl;
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var options = sp.GetRequiredService<IOptions<MastercardOptions>>().Value;
    var handler = new HttpClientHandler();

    if (options.AuthenticationMode == MastercardAuthenticationMode.MutualTls)
    {
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ClientCertificates.Add(
            sp.GetRequiredService<IMastercardCertificateProvider>()
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
app.MapPost("/callbacks/mastercard/{eventType}",
    async (
        string eventType,
        HttpRequest request,
        IMastercardCallbackValidator validator,
        IMastercardCallbackQueue queue,
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

The concrete acknowledgement contract must follow the selected Mastercard API's callback specification.

## 14. Error handling

Classify errors into:

- authentication;
- authorization;
- TLS;
- MLE;
- validation;
- rate limiting;
- Mastercard/client 4xx;
- Mastercard/server 5xx;
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
Running Mastercard adapter
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
- [ ] Correct Mastercard product identified.
- [ ] Regional availability confirmed.
- [ ] Restricted-access requirements identified.
- [ ] Sponsor/acquirer/processor dependencies known.

### Project
- [ ] Mastercard Developers account.
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

Mastercard Developers onboarding/security is infrastructure and network integration.

Keep the boundary:

```text
Dynamic Interchange Domain
        |
        v
Network-neutral decision
        |
        v
Mastercard Adapter
        |
        +-- Mastercard request mapping
        +-- MLE
        +-- Authentication
        +-- Mastercard endpoint
        +-- Callback handling
        |
        v
Mastercard
```

This prevents Mastercard-specific credentials, PKI and onboarding concepts from contaminating the generic interchange configuration model.

## 20. Review rule

The Mastercard Developers site and project-specific Assets are authoritative for current implementation details. Before each certification or production promotion, re-check the active product documentation, credentials, API reference, onboarding dashboard, and restricted assets rather than relying solely on this handbook.
