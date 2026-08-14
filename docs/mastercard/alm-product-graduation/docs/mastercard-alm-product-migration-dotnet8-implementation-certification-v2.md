# Mastercard Account Level Management (ALM) / Account Catalog Services
## .NET 8 integration add-on: move an individual PAN between products/programs without changing PAN or BIN

> Status: implementation reference based on Mastercard's public Account Catalog Services / Account Level Management documentation.  
> Important: bind the request/response DTOs and exact resource path to the OpenAPI specification provisioned in your Mastercard Developers project before production use. Mastercard exposes product-specific schemas and entitlements that can differ by programme.

## 1. Requirement

Support migration of one individual Mastercard account from one product/program identifier to another while:

- keeping the same physical/logical card account;
- keeping the same PAN;
- therefore keeping the same BIN/account range;
- changing the product/program treatment at Account Level Management level;
- avoiding card reissue purely for the product migration;
- retaining a full internal audit trail of old product, new product, reason, request ID and Mastercard response.

Example:

```text
Before
PAN:              555555******4444
BIN/account range: 555555
Program/Product:  PRODUCT_STANDARD

After
PAN:              555555******4444
BIN/account range: 555555
Program/Product:  PRODUCT_PREMIUM
```

The PAN and BIN do not change. Only the ALM/Account Catalog Services product configuration for the PAN is updated.

## 2. Mastercard capability

The relevant Mastercard platform is **Account Catalog Services (ACS)** backed by **Account Level Management (ALM)**.

Mastercard describes ALM as enabling specialized processing at the individual card-account level.

For this use case, Mastercard documents **Product Graduation Plus (PGP)**. Mastercard's public documentation describes it as enabling seamless cardholder migration while maintaining the same 16-digit card number across card programs. Mastercard also publishes a specific testing scenario named **"Updating a PAN Product Code"** / **"Update an existing PAN's Product Graduating Product Code"**.

This is materially different from:

- lost/stolen PAN replacement;
- BIN migration;
- card reissuance;
- token replacement;
- changing the funding PAN in Mastercard One Credential.

The application should model this as an **account-level product migration**.

## 3. Official Mastercard documentation

Primary sources:

- Account Catalog Services documentation  
  https://developer.mastercard.com/account-catalog-services/documentation

- Account Catalog Services API Reference  
  https://developer.mastercard.com/account-catalog-services/documentation/api-reference/

- Account Catalog Services API specification page  
  https://developer.mastercard.com/account-catalog-services/documentation/api-reference/account-catalog-services-api/

- Product Graduating a Primary Account Number (PAN)  
  https://developer.mastercard.com/account-catalog-services/documentation/use-cases/pan-registration/product_graduating_pan/

- Updating a PAN Product Code  
  https://developer.mastercard.com/account-catalog-services/documentation/testing/pan-registration-bau-depricated/product-graduating-a-pan/scenario8/

- Account Catalog Services Quick Start  
  https://developer.mastercard.com/account-catalog-services/documentation/quick-start-guide/

- Mastercard OAuth 1.0a C# signer  
  https://github.com/Mastercard/oauth1-signer-csharp

- Mastercard security/authentication documentation  
  https://developer.mastercard.com/platform/documentation/security-and-authentication/

Mastercard documents ACS sandbox and production under the Mastercard API gateway, including the ACS base path under `/asc/acs-api`.

## 4. Architecture

```text
Client / Back-office
        |
        v
Card Product Migration API
        |
        +--> Eligibility / business validation
        |
        +--> Product mapping
        |       InternalProgramId -> MastercardProductCode
        |
        +--> MastercardAlmClient
        |       OAuth 1.0a signing
        |       ACS / ALM request
        |
        +--> Migration audit store
        |
        +--> metrics / logs / alerts
```

Recommended internal components:

```text
Application
  CardProductMigrationService

Domain
  CardProductMigration
  ProductMapping
  MigrationStatus

Infrastructure.Mastercard
  MastercardAlmClient
  MastercardOAuthSigningHandler
  MastercardAlmOptions
  MastercardAlmContractMapper

Persistence
  CardProductMigrationRepository
```

## 5. API exposed by our application

Do not expose Mastercard's raw ACS contract to consumers.

Example internal API:

```http
POST /api/v1/cards/{cardId}/product-migrations
Idempotency-Key: 72ad7fc1-3219-4ccc-b448-b39afb075539
Content-Type: application/json
```

```json
{
  "targetProgramId": "PREMIUM_01",
  "reason": "CUSTOMER_UPGRADE",
  "effectiveImmediately": true
}
```

Response:

```json
{
  "migrationId": "01K2ABCDEF3XYZ",
  "cardId": "card_123",
  "maskedPan": "555555******4444",
  "bin": "555555",
  "previousProgramId": "STANDARD_01",
  "targetProgramId": "PREMIUM_01",
  "status": "Completed"
}
```

Never accept a clear PAN from a browser/UI unless the architecture and PCI scope explicitly require it. Resolve `cardId -> PAN` inside the PCI-controlled card domain.

## 6. .NET 8 project

```bash
dotnet new webapi -n CardProductMigration.Api -f net8.0
cd CardProductMigration.Api

dotnet add package Mastercard.Developer.OAuth1Signer.Core
dotnet add package Microsoft.Extensions.Http.Resilience
```

The Mastercard-maintained OAuth signer targets .NET Standard and can be consumed by .NET 8.

## 7. Configuration

```json
{
  "MastercardAlm": {
    "BaseUrl": "https://sandbox.api.mastercard.com",
    "AcsBasePath": "/asc/acs-api",
    "ConsumerKey": "",
    "SigningKeyPath": "",
    "SigningKeyAlias": "",
    "SigningKeyPassword": "",
    "PanProductUpdateRelativePath": ""
  }
}
```

`PanProductUpdateRelativePath` is intentionally configuration-driven.

**Do not hard-code a guessed path.** Populate it from the ACS OpenAPI/API Reference version enabled for your Mastercard project.

Production:

```json
{
  "MastercardAlm": {
    "BaseUrl": "https://api.mastercard.com"
  }
}
```

Secrets must come from AWS Secrets Manager / Parameter Store, Azure Key Vault, Kubernetes Secrets backed by a secret manager, or equivalent. Never commit the P12/PFX signing key or password.

## 8. Options

```csharp
public sealed class MastercardAlmOptions
{
    public const string SectionName = "MastercardAlm";

    public required string BaseUrl { get; init; }
    public string AcsBasePath { get; init; } = "/asc/acs-api";

    public required string ConsumerKey { get; init; }
    public required string SigningKeyPath { get; init; }
    public required string SigningKeyAlias { get; init; }
    public required string SigningKeyPassword { get; init; }

    // Copy from the Mastercard ACS OpenAPI contract enabled for the project.
    public required string PanProductUpdateRelativePath { get; init; }
}
```

## 9. Domain request

```csharp
public sealed record MoveCardToProductCommand(
    string CardId,
    string TargetProgramId,
    string Reason,
    string IdempotencyKey);

public sealed record CardAccount(
    string CardId,
    string Pan,
    string MaskedPan,
    string Bin,
    string CurrentProgramId);

public sealed record MastercardProductMapping(
    string InternalProgramId,
    string MastercardProductCode);
```

## 10. Mastercard contract boundary

Keep Mastercard DTOs isolated from the domain.

The exact ACS field names must be generated/copied from the Mastercard OpenAPI definition enabled for your project.

```csharp
public sealed record AlmPanProductUpdate(
    string Pan,
    string TargetProductCode);

public sealed record AlmPanProductUpdateResult(
    bool Accepted,
    string? MastercardRequestId,
    string? ProductRuleId,
    string RawResponse);
```

The `Pan` and `TargetProductCode` members above are **domain-side adapter names**, not a claim that these are the exact serialized ACS property names.

Use a mapper generated against the Mastercard schema:

```csharp
public interface IMastercardAlmContractMapper
{
    HttpContent CreatePanProductUpdateContent(AlmPanProductUpdate update);
    Task<AlmPanProductUpdateResult> ParsePanProductUpdateResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken);
}
```

This prevents Mastercard contract changes from leaking into the application/domain layer.

## 11. OAuth 1.0a signing handler

Mastercard provides an official C# OAuth 1.0a signer.

```csharp
using System.Security.Cryptography;
using Mastercard.Developer.OAuth1Signer.Core;

public sealed class MastercardOAuthSigningHandler : DelegatingHandler
{
    private readonly NetHttpClientSigner _signer;

    public MastercardOAuthSigningHandler(
        string consumerKey,
        RSA signingKey)
    {
        _signer = new NetHttpClientSigner(consumerKey, signingKey);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _signer.Sign(request);
        return base.SendAsync(request, cancellationToken);
    }
}
```

Load the Mastercard signing key using the helper exposed by the official library:

```csharp
var signingKey = AuthenticationUtils.LoadSigningKey(
    options.SigningKeyPath,
    options.SigningKeyAlias,
    options.SigningKeyPassword);
```

## 12. Mastercard ALM client

```csharp
public interface IMastercardAlmClient
{
    Task<AlmPanProductUpdateResult> UpdatePanProductAsync(
        AlmPanProductUpdate request,
        CancellationToken cancellationToken);
}
```

```csharp
using Microsoft.Extensions.Options;

public sealed class MastercardAlmClient : IMastercardAlmClient
{
    private readonly HttpClient _httpClient;
    private readonly MastercardAlmOptions _options;
    private readonly IMastercardAlmContractMapper _mapper;

    public MastercardAlmClient(
        HttpClient httpClient,
        IOptions<MastercardAlmOptions> options,
        IMastercardAlmContractMapper mapper)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _mapper = mapper;
    }

    public async Task<AlmPanProductUpdateResult> UpdatePanProductAsync(
        AlmPanProductUpdate request,
        CancellationToken cancellationToken)
    {
        var content = _mapper.CreatePanProductUpdateContent(request);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            _options.PanProductUpdateRelativePath)
        {
            Content = content
        };

        httpRequest.Headers.Accept.ParseAdd("application/json");

        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var result =
            await _mapper.ParsePanProductUpdateResponseAsync(
                response,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new MastercardAlmException(
                response.StatusCode,
                result.RawResponse);
        }

        return result;
    }
}
```

Do not assume `PUT` if your provisioned ACS contract says otherwise. Mastercard's public scenarios include update operations, but the exact method/resource must come from the current ACS specification for your project. If the provisioned operation is `POST`, change only this infrastructure adapter.

## 13. Service registration

```csharp
using System.Security.Cryptography;
using Mastercard.Developer.OAuth1Signer.Core;
using Microsoft.Extensions.Options;

builder.Services
    .AddOptions<MastercardAlmOptions>()
    .BindConfiguration(MastercardAlmOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<RSA>(sp =>
{
    var options = sp
        .GetRequiredService<IOptions<MastercardAlmOptions>>()
        .Value;

    return AuthenticationUtils.LoadSigningKey(
        options.SigningKeyPath,
        options.SigningKeyAlias,
        options.SigningKeyPassword);
});

builder.Services.AddTransient<MastercardOAuthSigningHandler>(sp =>
{
    var options = sp
        .GetRequiredService<IOptions<MastercardAlmOptions>>()
        .Value;

    var key = sp.GetRequiredService<RSA>();

    return new MastercardOAuthSigningHandler(
        options.ConsumerKey,
        key);
});

builder.Services
    .AddHttpClient<IMastercardAlmClient, MastercardAlmClient>((sp, client) =>
    {
        var options = sp
            .GetRequiredService<IOptions<MastercardAlmOptions>>()
            .Value;

        client.BaseAddress = new Uri(
            options.BaseUrl.TrimEnd('/') +
            options.AcsBasePath.TrimEnd('/') + "/");

        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<MastercardOAuthSigningHandler>()
    .AddStandardResilienceHandler();
```

## 14. Product-migration orchestration

```csharp
public sealed class CardProductMigrationService
{
    private readonly ICardRepository _cards;
    private readonly IProductMappingRepository _productMappings;
    private readonly IMastercardAlmClient _mastercard;
    private readonly IMigrationRepository _migrations;

    public CardProductMigrationService(
        ICardRepository cards,
        IProductMappingRepository productMappings,
        IMastercardAlmClient mastercard,
        IMigrationRepository migrations)
    {
        _cards = cards;
        _productMappings = productMappings;
        _mastercard = mastercard;
        _migrations = migrations;
    }

    public async Task<CardProductMigrationResult> MoveAsync(
        MoveCardToProductCommand command,
        CancellationToken cancellationToken)
    {
        var existing =
            await _migrations.FindByIdempotencyKeyAsync(
                command.IdempotencyKey,
                cancellationToken);

        if (existing is not null)
            return existing.ToResult();

        var card = await _cards.GetAsync(
            command.CardId,
            cancellationToken);

        if (card is null)
            throw new InvalidOperationException("Card not found.");

        if (card.CurrentProgramId == command.TargetProgramId)
            return CardProductMigrationResult.NoChange(
                card.CardId,
                card.MaskedPan,
                card.Bin,
                card.CurrentProgramId);

        var target =
            await _productMappings.GetAsync(
                command.TargetProgramId,
                cancellationToken);

        if (target is null)
            throw new InvalidOperationException(
                "Target programme has no Mastercard product mapping.");

        // Critical invariant:
        // this operation does NOT replace or mutate the PAN.
        var panBefore = card.Pan;
        var binBefore = card.Bin;

        var migration = CardProductMigration.Start(
            card,
            command.TargetProgramId,
            command.Reason,
            command.IdempotencyKey);

        await _migrations.InsertAsync(migration, cancellationToken);

        try
        {
            var mcResult = await _mastercard.UpdatePanProductAsync(
                new AlmPanProductUpdate(
                    Pan: card.Pan,
                    TargetProductCode: target.MastercardProductCode),
                cancellationToken);

            // Verify local invariants.
            if (card.Pan != panBefore || card.Bin != binBefore)
                throw new InvalidOperationException(
                    "PAN/BIN invariant violated.");

            migration.MarkCompleted(
                mcResult.MastercardRequestId,
                mcResult.ProductRuleId);

            await _cards.ChangeProgramAsync(
                card.CardId,
                command.TargetProgramId,
                cancellationToken);

            await _migrations.UpdateAsync(
                migration,
                cancellationToken);

            return migration.ToResult();
        }
        catch (Exception ex)
        {
            migration.MarkFailed(ex.GetType().Name);

            await _migrations.UpdateAsync(
                migration,
                cancellationToken);

            throw;
        }
    }
}
```

## 15. API endpoint

```csharp
public sealed record MoveCardProductRequest(
    string TargetProgramId,
    string Reason);

app.MapPost(
    "/api/v1/cards/{cardId}/product-migrations",
    async (
        string cardId,
        MoveCardProductRequest body,
        HttpRequest request,
        CardProductMigrationService service,
        CancellationToken cancellationToken) =>
    {
        if (!request.Headers.TryGetValue(
                "Idempotency-Key",
                out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return Results.BadRequest(
                new { error = "Idempotency-Key is required." });
        }

        var result = await service.MoveAsync(
            new MoveCardToProductCommand(
                cardId,
                body.TargetProgramId,
                body.Reason,
                idempotencyKey!),
            cancellationToken);

        return Results.Ok(result);
    });
```

## 16. Example business flow

```text
1. User requests upgrade:
   STANDARD_01 -> PREMIUM_01

2. System resolves:
   cardId = card_123
   PAN = 555555******4444
   BIN = 555555
   current Mastercard product = MCS
   target Mastercard product = MCW

3. Validate:
   - target product configured;
   - same Mastercard account range is eligible;
   - ALM/PGP enabled for issuer/account range;
   - no migration already in progress;
   - PAN active;
   - target programme allowed by business rules.

4. Call Mastercard Account Catalog Services / ALM.

5. Mastercard validates PAN/product and updates Product Graduation configuration.

6. Persist:
   old programme = STANDARD_01
   new programme = PREMIUM_01
   PAN hash/token reference = unchanged
   BIN = unchanged
   Mastercard correlation identifiers
   timestamp
   reason
   actor

7. Return Completed/Pending/Rejected according to the ACS response.
```

## 17. Database model

```sql
create table card_product_migration
(
    migration_id              uuid primary key,
    card_id                   varchar(100) not null,
    idempotency_key           varchar(100) not null unique,

    source_program_id         varchar(100) not null,
    target_program_id         varchar(100) not null,

    source_product_code       varchar(100),
    target_product_code       varchar(100) not null,

    bin                       varchar(12) not null,
    pan_fingerprint           varchar(128) not null,

    status                    varchar(30) not null,
    reason                    varchar(100) not null,

    mastercard_request_id     varchar(200),
    mastercard_product_rule_id varchar(200),

    created_utc               timestamptz not null,
    completed_utc             timestamptz,
    failure_code              varchar(100)
);
```

Do **not** persist clear PAN in this table. Use a card ID plus token/fingerprint or fetch the PAN from a PCI-controlled card vault when the Mastercard call is made.

## 18. Idempotency

The migration endpoint must be idempotent.

Key:

```text
(cardId, targetProgramId, business-operation-id)
```

Recommended behaviour:

```text
same key + same payload + Completed -> return original result
same key + same payload + Processing -> return current state
same key + different payload          -> 409 Conflict
different key + same target already active -> NoChange
```

Do not blindly retry write calls after an ambiguous network timeout. First determine whether Mastercard processed the operation, using the ACS retrieval/status capability available in your provisioned API contract.

## 19. Resilience

Recommended:

```text
DNS/connect failure            retry cautiously
HTTP 408                       retry cautiously
HTTP 429                       retry using Retry-After
HTTP 5xx                       bounded exponential retry
OAuth/signature 401/403        do not retry blindly
business validation 4xx        do not retry
ambiguous write timeout        reconcile before retry
```

For a PAN-level configuration write, at-most-once business behaviour is more important than an aggressive generic HTTP retry policy.

## 20. Observability

Metrics:

```text
mastercard_alm_requests_total
mastercard_alm_request_duration_ms
mastercard_alm_errors_total
mastercard_product_migrations_total
mastercard_product_migrations_failed_total
mastercard_product_migrations_pending_total
mastercard_product_migration_reconciliation_total
```

Log:

```text
migrationId
cardId
BIN
sourceProgramId
targetProgramId
sourceProductCode
targetProductCode
Mastercard correlation/request ID
HTTP status
latency
```

Never log:

```text
full PAN
CVV/CVC
PIN/PIN block
private signing key
OAuth Authorization header
unencrypted sensitive Mastercard payload
```

## 21. Security / PCI

Because PAN is sent to Mastercard, treat the integration as a PCI-sensitive boundary.

Required controls:

- TLS only;
- Mastercard OAuth credentials held in a secret manager;
- signing key access restricted to the service identity;
- no PAN in application logs/traces;
- no PAN in exception messages;
- masking at UI/API boundaries;
- structured log redaction;
- short-lived in-memory PAN handling;
- least-privilege service account;
- audit record for every migration;
- dual-control/approval if business policy requires it;
- network egress restricted to Mastercard API endpoints;
- certificate/key rotation procedure;
- access reviews;
- threat model for account/product manipulation;
- monitoring for abnormal bulk migration volume.

## 22. Product eligibility must be configuration-driven

Do not let callers submit arbitrary Mastercard product codes.

```json
{
  "programMappings": [
    {
      "internalProgramId": "STANDARD_01",
      "mastercardProductCode": "VALUE_FROM_MASTERCARD_CONFIGURATION",
      "almEnabled": true
    },
    {
      "internalProgramId": "PREMIUM_01",
      "mastercardProductCode": "VALUE_FROM_MASTERCARD_CONFIGURATION",
      "almEnabled": true
    }
  ]
}
```

The actual product codes/program identifiers must come from issuer/Mastercard configuration and certification material.

## 23. Same-BIN guard

The business requirement explicitly says same BIN.

Validate before calling Mastercard:

```csharp
public static void EnsureSameBinMigration(
    CardAccount card,
    TargetProgramme target)
{
    if (!target.AllowedBins.Contains(card.Bin))
    {
        throw new InvalidOperationException(
            $"Target programme is not enabled for BIN {card.Bin}.");
    }
}
```

This ensures an accidental cross-BIN migration is rejected before reaching ACS.

## 24. State machine

```text
Requested
   |
   v
Validated
   |
   v
SubmittedToMastercard
   |          \
   |           \--> Rejected
   v
Completed

SubmittedToMastercard
   |
   +--> Unknown -> Reconciling -> Completed / Rejected / ManualReview
```

Do not mark the internal card programme changed until the Mastercard result is confirmed, unless the Mastercard flow is explicitly asynchronous and your state model represents that.

## 25. Tests

### Unit tests

```text
- same PAN before and after migration
- same BIN before and after migration
- target product mapping required
- unsupported BIN rejected
- no-op when already on target programme
- duplicate idempotency key does not call Mastercard twice
- PAN never appears in structured logs
- Mastercard validation failure does not mutate local programme
- ambiguous Mastercard response moves state to reconciliation
```

### Integration tests

Use Mastercard sandbox test data from the ACS testing pages applicable to the enabled programme.

Test:

```text
STANDARD -> PREMIUM
PREMIUM -> STANDARD (if permitted)
same product -> no-op
invalid target product
inactive/closed PAN
non-enabled PAN
non-enabled account range
invalid OAuth signature
duplicate migration
Mastercard timeout
HTTP 429
HTTP 5xx
```

## 26. Recommended OpenAPI-generated client option

For the most contract-safe implementation:

1. Download the ACS OpenAPI specification available to the Mastercard Developers project.
2. Generate the .NET client.
3. Add Mastercard OAuth signing as an HTTP/RestSharp interceptor.
4. Wrap the generated API in `IMastercardAlmClient`.
5. Never expose generated Mastercard models outside Infrastructure.

Mastercard's official OAuth signer explicitly documents integration with OpenAPI Generator and C#/.NET clients.

Conceptual generation command:

```bash
openapi-generator-cli generate \
  -i mastercard-account-catalog-services.yaml \
  -g csharp \
  -o ./Generated/Mastercard.AccountCatalog
```

Use the generator/version required by the current Mastercard specification and your repository standards.

## 27. Acceptance criteria

- [ ] Individual card can migrate from Product/Programme A to Product/Programme B.
- [ ] PAN is identical before and after migration.
- [ ] BIN/account range is identical before and after migration.
- [ ] No card reissue is triggered by this operation.
- [ ] Target programme is mapped to a Mastercard-configured product.
- [ ] Target programme is allowed for the existing BIN.
- [ ] Mastercard ACS/ALM is the system-of-record integration for network product graduation.
- [ ] OAuth 1.0a requests are signed using Mastercard-supported signing logic.
- [ ] Exact ACS operation and DTOs are generated/bound from the provisioned Mastercard OpenAPI contract.
- [ ] Operation is idempotent.
- [ ] Ambiguous write outcomes enter reconciliation rather than blind retry.
- [ ] Full PAN is absent from logs, traces and audit tables.
- [ ] Migration keeps Mastercard request/correlation identifiers.
- [ ] Metrics and alerts exist.
- [ ] Sandbox certification covers upgrade, downgrade, invalid product and failure scenarios.
- [ ] Production rollout supports feature flags and rollback of the application-side mapping.

## 28. Implementation recommendation

Treat this as a new capability inside the existing card-product orchestration layer:

```text
ICardProductNetworkAdapter
    |
    +-- Visa implementation
    |
    +-- MastercardAlmAdapter
```

Public interface:

```csharp
public interface ICardProductNetworkAdapter
{
    Task<NetworkProductMigrationResult> MoveProductAsync(
        NetworkProductMigration request,
        CancellationToken cancellationToken);
}
```

This keeps business logic network-neutral while allowing Mastercard-specific Account Catalog Services / ALM rules, signing, contract versions and certification to live behind the Mastercard adapter.

## 29. Definition of Done

The feature is complete only after:

1. Mastercard confirms ACS/ALM + Product Graduation Plus entitlement for the issuer/program.
2. The exact ACS OpenAPI file is attached to the implementation/version controlled.
3. Product codes and eligible account ranges are confirmed.
4. Sandbox calls pass with Mastercard-provided test data.
5. Same-PAN/same-BIN migration is proven end-to-end.
6. Error/reconciliation scenarios pass.
7. PCI/security review passes.
8. Mastercard certification/production approval is completed where required.
9. Production credentials/certificates are deployed.
10. Feature is enabled gradually with migration telemetry and alerting.


---

# 30. Mastercard verification, onboarding and certification plan

This section turns the integration into an executable onboarding and evidence plan for Mastercard Account Catalog Services (ACS), Account Level Management (ALM), and Product Graduation Plus (PGP).

## 30.1 What Mastercard publicly confirms

Mastercard's Account Catalog Services documentation states that Product Graduation Plus allows an issuer to migrate a cardholder across product codes without changing the PAN. For the PAN-level use case, the issuer provides the PAN and the product to which the account is being upgraded or downgraded, and ALM validates the PAN/product before applying the configuration.

Mastercard also publishes testing navigation for:

- Product Graduating for a new PAN;
- updating an existing PAN's Product Graduating Product Code;
- deleting a PAN from Product Graduation Service;
- Product Graduation for eligible high-value accounts;
- account-range enablement for Product Graduation Plus;
- detailed response/query capabilities.

Official documentation:

- Account Catalog Services:
  https://developer.mastercard.com/account-catalog-services/documentation

- Product Graduating a PAN:
  https://developer.mastercard.com/account-catalog-services/documentation/use-cases/pan-registration/product_graduating_pan/

- ACS API Reference:
  https://developer.mastercard.com/account-catalog-services/documentation/api-reference/

- ACS PAN API:
  https://developer.mastercard.com/account-catalog-services/documentation/api-reference/account-catalog-services-api/

- Account Range Enablement API:
  https://developer.mastercard.com/account-catalog-services/documentation/api-reference/account-enablement-api/

- Detail Response API:
  https://developer.mastercard.com/account-catalog-services/documentation/api-reference/account-catalog-services-detail-response-api/

- ACS API Basics:
  https://developer.mastercard.com/account-catalog-services/documentation/api-basics/

- ACS Quick Start:
  https://developer.mastercard.com/account-catalog-services/documentation/quick-start-guide/

- ACS Support:
  https://developer.mastercard.com/account-catalog-services/documentation/support/

## 30.2 Mastercard authentication

Mastercard Developers documents one-legged OAuth 1.0a as a supported/default authentication mechanism for many Mastercard APIs.

Official references:

- OAuth 1.0a:
  https://developer.mastercard.com/platform/documentation/authentication/using-oauth-1a-to-access-mastercard-apis/

- Authentication overview:
  https://developer.mastercard.com/platform/documentation/authentication/

- OAuth key management:
  https://developer.mastercard.com/platform/documentation/credential-management/oauth-key-management/

- Credential management:
  https://developer.mastercard.com/platform/documentation/credential-management/

- OAuth/mTLS errors:
  https://developer.mastercard.com/platform/documentation/errors-and-troubleshooting/oauth1-and-mtls-error-codes/

- Official C# OAuth signer:
  https://github.com/Mastercard/oauth1-signer-csharp

OAuth 2.0 support is being added to Mastercard APIs, but the exact authentication mechanism for the ACS project must be taken from the enabled project/API contract rather than assumed globally.

## 30.3 Required Mastercard confirmations before implementation freeze

Open a Mastercard support/onboarding request and obtain written confirmation of all of the following:

1. The issuer/processor is entitled to **Account Catalog Services**.
2. **Account Level Management** is enabled for the relevant issuer relationship.
3. **Product Graduation Plus** is enabled.
4. The target BIN/account ranges are enabled or can be enabled for PGP.
5. The intended source and destination Mastercard product codes are valid.
6. The same PAN is allowed to graduate between those two products.
7. The migration is valid without issuing a replacement PAN/card.
8. Any regional or scheme-specific restrictions on upgrade/downgrade are understood.
9. Whether account-range enablement must be completed before PAN registration/update.
10. Which API version must be used.
11. Which exact PAN update operation is currently supported.
12. Whether the operation is synchronous or requires reconciliation/status lookup.
13. Which response identifier should be retained as certification evidence.
14. Whether MTF/formal certification is mandatory for this issuer/processor/change.
15. Which formal test cases Mastercard expects.
16. Production cutover prerequisites.
17. Rollback/restore mechanism expected by Mastercard.

Store Mastercard's answers in the implementation repository under:

```text
/docs/mastercard/alm/
    entitlement-confirmation.md
    product-code-mapping.md
    account-range-confirmation.md
    api-version.md
    certification-requirements.md
```

Do not put confidential credentials, PANs, Mastercard proprietary test data, or restricted documentation in a public repository.

# 31. Environment model

Use four logical environments even if Mastercard exposes a smaller number of physical endpoints.

```text
LOCAL
  |
  +-- mocked IMastercardAlmClient
  |
DEV / CI
  |
  +-- contract tests
  +-- OAuth signing tests
  |
MASTERCARD SANDBOX / MTF
  |
  +-- Mastercard-provided accounts/PAN test data
  +-- ACS integration tests
  +-- certification evidence
  |
PRODUCTION
  |
  +-- production consumer key
  +-- production certificate/private key
  +-- production ACS entitlement
```

Mastercard's ACS documentation indicates that customers should contact their Mastercard representative and regional CIS team for Sandbox/MTF access/provisioning when required.

Therefore:

- a Mastercard Developers project proves API connectivity;
- Sandbox tests prove technical integration;
- MTF/formal testing proves issuer/program behavior where Mastercard requires it;
- production entitlement is a separate go-live gate.

# 32. Mastercard onboarding workflow

## Phase MC-01 — Business entitlement

Owner:
Product / Cards / Mastercard relationship manager

Actions:

- identify issuing ICA/customer identifiers;
- identify BIN/account ranges;
- identify current Mastercard product code;
- identify destination Mastercard product code;
- document upgrade and downgrade scenarios;
- request ACS + ALM + Product Graduation Plus access;
- confirm whether Account Range Enablement is required;
- request Sandbox/MTF access through Mastercard representative/CIS.

Evidence:

```text
MC-01-01 Mastercard entitlement confirmation
MC-01-02 PGP enablement confirmation
MC-01-03 BIN/account-range list
MC-01-04 approved source/target product mapping
MC-01-05 certification requirement confirmation
```

Gate:

```text
GO only when Mastercard confirms that the requested source-product
-> target-product movement is supported for the same PAN/account range.
```

## Phase MC-02 — Mastercard Developers project

Actions:

1. Create or update the Mastercard Developers project.
2. Add Account Catalog Services.
3. Generate authentication credentials.
4. Secure the private key.
5. Record the consumer key/credential ID in the secret-management system.
6. Download or capture the exact API/OpenAPI specification exposed to the project.
7. Record API version and release date.
8. Confirm Sandbox base URL.
9. Confirm Production base URL from the project/docs.
10. Run a signed connectivity test.

Evidence:

```text
MC-02-01 project identifier
MC-02-02 ACS product attached
MC-02-03 credential creation evidence
MC-02-04 API/OpenAPI version
MC-02-05 successful signed Sandbox request
```

Never store the private signing key in certification evidence.

## Phase MC-03 — Contract freeze

Create:

```text
/src/Infrastructure/Mastercard/Generated/
```

Generate or bind the Mastercard client from the ACS API definition.

Freeze:

```text
Mastercard product: Account Catalog Services
API version: <confirmed version>
OpenAPI checksum: <sha256>
Authentication: <OAuth1 / OAuth2 / mTLS as provisioned>
PAN update operation: <exact operationId>
HTTP method: <exact method>
Path: <exact path>
Request DTO: <exact schema>
Response DTO: <exact schema>
Error DTO: <exact schema>
```

This is the point at which placeholder DTOs and placeholder paths from the architecture document must be replaced.

Gate:

```text
No production implementation proceeds with guessed API fields,
methods, paths or product codes.
```

# 33. Account-range enablement

Mastercard publishes a separate Account Range Enablement API for enabling relevant account ranges for Product Graduation Plus.

Before PAN-level migration:

```text
Issuer/Processor
       |
       v
Is Account Range enabled for PGP?
       |
    +--+--+
    |     |
   YES    NO
    |     |
    |     v
    |  Account Range Enablement
    |     |
    +-----+
       |
       v
PAN Product Graduation
```

Implementation recommendation:

```csharp
public interface IMastercardAlmEligibilityService
{
    Task<AlmEligibilityResult> VerifyAsync(
        string binOrAccountRangeReference,
        string sourceProduct,
        string targetProduct,
        CancellationToken cancellationToken);
}
```

Do not dynamically enable account ranges during an end-user card upgrade unless Mastercard and issuer operations explicitly approve that workflow.

Account-range setup is normally an administrative/network configuration concern and should be kept separate from a single-card business transaction.

# 34. Verification API strategy

Where available in the entitled ACS API version, use the detail/query response capability to verify Mastercard state.

Expected orchestration:

```text
1. Read current internal state.
2. Validate source/target product.
3. Submit ACS PAN product graduation/update.
4. Capture request/correlation identifier.
5. Read/verify resulting ACS state.
6. Compare expected target product.
7. Commit local state.
8. Persist evidence.
```

For uncertain outcomes:

```text
Submit
  |
  +--> confirmed success -> verify -> complete
  |
  +--> confirmed reject -> failed
  |
  +--> timeout / connection reset / unknown
                         |
                         v
                    reconciliation
                         |
                         v
                    query Mastercard
                         |
                    +----+----+
                    |         |
                  applied   not applied
                    |         |
                    v         v
                 complete   controlled retry
```

Never use an automatic retry after an ambiguous write unless the API contract/documentation explicitly guarantees safe idempotency.

# 35. Internal .NET 8 verification component

```csharp
public interface IMastercardAlmVerificationClient
{
    Task<MastercardPanProductState> GetPanProductStateAsync(
        string pan,
        CancellationToken cancellationToken);
}

public sealed record MastercardPanProductState(
    string ProductCode,
    string? RuleId,
    string? MastercardRequestId);
```

Migration:

```csharp
var before = await verification.GetPanProductStateAsync(
    pan,
    cancellationToken);

if (before.ProductCode != expectedSourceProduct)
{
    throw new ProductStateMismatchException(
        expectedSourceProduct,
        before.ProductCode);
}

var writeResult = await alm.UpdatePanProductAsync(
    new AlmPanProductUpdate(pan, targetProduct),
    cancellationToken);

var after = await verification.GetPanProductStateAsync(
    pan,
    cancellationToken);

if (after.ProductCode != targetProduct)
{
    await reconciliation.EnqueueAsync(
        migrationId,
        cancellationToken);

    throw new MastercardStateNotConfirmedException(
        migrationId);
}
```

Use the exact query/detail operation exposed by the project. The code above defines the application behavior, not Mastercard's serialized schema.

# 36. Certification test pack

Create one evidence folder per run:

```text
/certification/mastercard-alm/<run-id>/
    00-scope.md
    01-environment.md
    02-product-mapping.md
    03-test-data-index.md
    04-test-cases.md
    05-results.json
    06-request-response-evidence/
    07-observability-evidence/
    08-security-evidence/
    09-defects.md
    10-signoff.md
```

All PAN evidence must be masked/tokenized according to PCI and Mastercard requirements.

## MC-ALM-001 — Register/graduate eligible PAN

Precondition:

```text
PAN eligible for PGP
Account range enabled
Source product = PRODUCT_A
Target product = PRODUCT_B
```

Expected:

```text
Mastercard accepts operation
PAN unchanged
Account range/BIN unchanged
Mastercard resulting state = PRODUCT_B
Local state = PRODUCT_B
Correlation identifier stored
```

## MC-ALM-002 — Upgrade existing PAN product code

```text
PRODUCT_A -> PRODUCT_B
```

Verify:

- successful API response;
- same PAN;
- same BIN;
- resulting Mastercard product code;
- local mapping updated exactly once.

## MC-ALM-003 — Downgrade

```text
PRODUCT_B -> PRODUCT_A
```

Only run if the exact product pair and downgrade are approved by Mastercard.

## MC-ALM-004 — PAN already target product

Expected:

```text
no duplicate network mutation
local result = NoChange
```

or the exact Mastercard-defined behavior if certification requires calling the network.

## MC-ALM-005 — Invalid target product

Expected:

```text
rejected
local product unchanged
no retry
error mapped
```

## MC-ALM-006 — Account range not enabled

Expected:

```text
rejected before call or rejected by Mastercard
local product unchanged
operational error emitted
```

## MC-ALM-007 — Invalid/unknown PAN

Expected:

```text
Mastercard validation rejection
no local state mutation
PAN absent from logs
```

## MC-ALM-008 — Duplicate request/business operation

Expected:

```text
one logical migration only
no accidental multiple product transitions
```

## MC-ALM-009 — OAuth invalid signature

Expected:

```text
401/403 or exact Mastercard gateway error
no retry storm
credentials/signature alarm
no sensitive Authorization header logged
```

## MC-ALM-010 — Rate limiting

Expected:

```text
429 handled
Retry-After honored when applicable
no state corruption
```

## MC-ALM-011 — Mastercard 5xx

Expected:

```text
bounded retry only when safe
unknown write results reconciled
```

## MC-ALM-012 — Timeout after request transmission

This is a critical test.

Expected:

```text
status = Unknown / Reconciling
do NOT immediately repeat mutation
query/verify Mastercard resulting state
complete OR controlled retry
```

## MC-ALM-013 — Same BIN invariant

Capture:

```text
BIN before
BIN after
```

Expected:

```text
before == after
```

## MC-ALM-014 — Same PAN invariant

Use a secure/tokenized comparison inside the PCI zone.

Expected:

```text
PAN before == PAN after
```

Do not place the clear PAN in test reports.

## MC-ALM-015 — Reversal / restore

If supported/approved:

```text
PRODUCT_A -> PRODUCT_B -> PRODUCT_A
```

Verify Mastercard state after each transition.

# 37. Certification evidence JSON

```json
{
  "testCaseId": "MC-ALM-002",
  "migrationId": "internal-migration-id",
  "environment": "MASTERCARD_MTF",
  "maskedPan": "555555******4444",
  "bin": "555555",
  "sourceProduct": "PRODUCT_A",
  "targetProduct": "PRODUCT_B",
  "mastercardRequestId": "masked-or-non-sensitive-id",
  "httpStatus": 200,
  "mastercardResult": "ACCEPTED",
  "verificationResult": {
    "samePan": true,
    "sameBin": true,
    "targetProductConfirmed": true
  },
  "startedUtc": "2026-08-14T10:00:00Z",
  "completedUtc": "2026-08-14T10:00:01Z",
  "result": "PASS"
}
```

Adapt the result/status fields to the exact Mastercard response.

# 38. Automated certification test harness

Recommended project:

```text
/tests/
    Mastercard.Alm.UnitTests/
    Mastercard.Alm.ContractTests/
    Mastercard.Alm.SandboxTests/
    Mastercard.Alm.CertificationTests/
```

Tag tests:

```csharp
[Trait("Mastercard", "ALM")]
[Trait("Environment", "MTF")]
public async Task Existing_pan_can_change_product_without_pan_change()
{
    // Arrange with Mastercard-approved certification data.

    // Act:
    // read before
    // execute graduation
    // read after

    // Assert:
    // target product changed
    // PAN fingerprint unchanged
    // BIN unchanged
}
```

Never put Mastercard MTF credentials or clear PAN data in source code.

# 39. CI/CD controls

Pipeline:

```text
Build
  |
Unit Tests
  |
Contract Tests
  |
SAST / secret scan
  |
Package
  |
Deploy DEV
  |
Integration Tests
  |
Manual approval
  |
Deploy Mastercard MTF
  |
Certification Tests
  |
Evidence package
  |
Mastercard / issuer approval
  |
Production change approval
  |
Deploy Production disabled
  |
Smoke test
  |
Feature flag enablement
```

Mandatory gate variables:

```text
MASTER_CARD_ALM_ENTITLEMENT_APPROVED=true
MASTER_CARD_ALM_MTF_PASSED=true
MASTER_CARD_ALM_BUSINESS_SIGNOFF=true
MASTER_CARD_ALM_SECURITY_SIGNOFF=true
MASTER_CARD_ALM_PRODUCTION_CREDENTIALS=true
```

Do not literally use environment variables as governance evidence; the example represents required pipeline controls.

# 40. Production rollout

Use a feature flag:

```text
MastercardAlmProductGraduationEnabled
```

Optional segmentation:

```text
MastercardAlmEnabledBins
MastercardAlmEnabledProducts
MastercardAlmTrafficPercentage
```

Recommended rollout:

```text
0%   deployed but disabled
internal/test accounts
pilot BIN/product pair
1-5%
25%
50%
100%
```

Observe at each stage:

- success rate;
- reject rate by code;
- unknown/reconciliation rate;
- latency;
- duplicate suppression;
- state mismatches;
- manual interventions.

Automatic kill switch:

```text
if reconciliation_rate > threshold
or Mastercard auth failures spike
or product-state mismatches > 0
then disable new ALM migrations
```

Do not automatically undo successfully graduated PANs unless Mastercard explicitly approves that rollback mechanism.

# 41. Production readiness review

## Functional

- [ ] Source and target product codes confirmed by Mastercard.
- [ ] Same-PAN Product Graduation confirmed.
- [ ] Same-BIN/account-range eligibility confirmed.
- [ ] Account range PGP enablement completed.
- [ ] Upgrade tested.
- [ ] Downgrade tested where supported.
- [ ] No-op/idempotency tested.
- [ ] Result verification implemented.
- [ ] Reconciliation implemented.

## API

- [ ] Exact ACS API version pinned.
- [ ] OpenAPI checksum stored.
- [ ] No guessed paths.
- [ ] No guessed request fields.
- [ ] No guessed response fields.
- [ ] Mastercard gateway errors mapped.
- [ ] Correlation IDs captured.
- [ ] Timeout policy reviewed.

## Security / PCI

- [ ] PCI scope documented.
- [ ] PAN access restricted.
- [ ] PAN redaction tests pass.
- [ ] OAuth Authorization header redacted.
- [ ] Private key in approved secret/HSM mechanism.
- [ ] Rotation tested.
- [ ] TLS validated.
- [ ] Egress allow-list configured.
- [ ] Security threat model completed.
- [ ] Access control / four-eyes approval evaluated.

## Operations

- [ ] Dashboards deployed.
- [ ] Alerts deployed.
- [ ] Runbook completed.
- [ ] Reconciliation queue monitored.
- [ ] Manual repair procedure exists.
- [ ] Mastercard support/escalation route documented.
- [ ] On-call knows kill-switch procedure.

## Mastercard

- [ ] Mastercard representative identified.
- [ ] Regional CIS contact/process confirmed.
- [ ] Sandbox/MTF access provisioned.
- [ ] Required Mastercard test cases confirmed.
- [ ] Evidence submitted if requested.
- [ ] Defects closed.
- [ ] Mastercard/issuer approval recorded.
- [ ] Production entitlement confirmed.
- [ ] Production credentials created.
- [ ] Production cutover approved.

# 42. Questions to send Mastercard before development completion

Use these questions in the Mastercard support case / issuer relationship channel:

```text
1. Please confirm that our issuer/processor relationship is entitled to
   Account Catalog Services, Account Level Management and Product Graduation Plus.

2. Please confirm that the following account ranges/BINs are eligible for
   PAN-level Product Graduation Plus.

3. Please confirm the Mastercard product codes for our source and destination
   card products.

4. Can an already-issued PAN be migrated from source product X to target
   product Y without changing the PAN?

5. Does this operation preserve the existing account range/BIN?

6. Which current ACS API operation and API version must be used to update an
   existing PAN's Product Graduation product code?

7. Please provide/confirm the authoritative OpenAPI specification applicable
   to our project.

8. Must the account range be explicitly enabled for PGP before PAN-level
   graduation?

9. What validation/certification is required in Sandbox/MTF before production?

10. Please provide the required test scenarios and expected evidence.

11. Is a detail/status API available to verify the effective product after an
    update and to reconcile ambiguous timeout outcomes?

12. What request/correlation identifier should we retain for support and
    certification?

13. What are the retry/idempotency expectations for PAN product updates?

14. Are downgrade/product restoration operations supported for our configured
    product pair?

15. Are there effective-date/batch-window constraints?

16. Are there regional or issuer-specific restrictions that are not represented
    in the public ACS documentation?

17. What is required to provision production credentials and enable production
    traffic?

18. Which Mastercard support queue/team should receive production incidents
    for ACS/ALM?
```

# 43. Responsibility matrix

| Area | Product | Engineering | Security | Issuer Ops | Mastercard |
|---|---|---|---|---|---|
| Product mapping | A | C | I | R | C |
| API implementation | I | R/A | C | I | C |
| OAuth credentials | I | R | A | I | C |
| Account range enablement | C | C | I | R | A/C |
| Sandbox tests | I | R/A | C | C | C |
| MTF certification | C | R | C | R | A/C |
| PCI review | I | C | R/A | C | I |
| Production approval | A | R | C | R | C |
| Incident escalation | I | R | C | R | C |

R = Responsible, A = Accountable, C = Consulted, I = Informed.

Adapt ownership to the issuer/processor operating model.

# 44. Recommended Jira / delivery epics

```text
EPIC MC-ALM-01 — Mastercard entitlement & onboarding
EPIC MC-ALM-02 — ACS OpenAPI client & authentication
EPIC MC-ALM-03 — Product mapping & eligibility
EPIC MC-ALM-04 — PAN Product Graduation orchestration
EPIC MC-ALM-05 — State verification & reconciliation
EPIC MC-ALM-06 — PCI/security controls
EPIC MC-ALM-07 — Observability & operations
EPIC MC-ALM-08 — Mastercard Sandbox/MTF certification
EPIC MC-ALM-09 — Production rollout
```

# 45. Suggested implementation order

```text
1. Mastercard entitlement confirmation
2. Product/BIN mapping confirmation
3. Obtain exact ACS OpenAPI
4. Create Mastercard Developers project/credentials
5. Generate .NET client
6. Implement OAuth signing
7. Implement contract tests
8. Implement eligibility guard
9. Implement PAN product graduation
10. Implement verification/query
11. Implement idempotency
12. Implement reconciliation
13. Implement PCI/log redaction
14. Add dashboards/alerts
15. Run Sandbox tests
16. Obtain MTF data/access
17. Run Mastercard-required certification
18. Submit evidence
19. Resolve findings
20. Obtain production approval/credentials
21. Deploy behind feature flag
22. Pilot
23. Progressive rollout
```

# 46. Final architecture

```text
                         +----------------------+
                         | Product / Backoffice |
                         +----------+-----------+
                                    |
                                    v
                         +----------------------+
                         | Product Migration API |
                         +----------+-----------+
                                    |
                  +-----------------+------------------+
                  |                                    |
                  v                                    v
        +--------------------+               +--------------------+
        | Eligibility/Mapping |               | Idempotency/Audit  |
        +---------+----------+               +--------------------+
                  |
                  v
        +----------------------+
        | Network Adapter      |
        +-----------+----------+
                    |
          +---------+----------+
          |                    |
          v                    v
 +----------------+    +----------------------+
 | Visa Adapter   |    | Mastercard ALM       |
 |                |    | ACS Adapter          |
 +----------------+    +----------+-----------+
                                  |
                                  | OAuth + ACS contract
                                  v
                         +----------------------+
                         | Mastercard ACS / ALM |
                         +----------+-----------+
                                    |
                        +-----------+------------+
                        |                        |
                        v                        v
                 Product Graduation      Detail/Verification
                        |
                        v
                 Same PAN / Same BIN
                 New Product Code
```

# 47. Important boundary

The public Mastercard documentation is sufficient to justify and design the capability, but it is **not sufficient to hard-code the final network contract**.

The authoritative production implementation must use:

```text
Mastercard project entitlement
+
current ACS OpenAPI specification
+
issuer-specific product codes
+
approved account ranges
+
Mastercard/CIS certification instructions
```

Any mismatch between this implementation guide and the Mastercard contract provisioned to the issuer must be resolved in favor of Mastercard's provisioned contract and written certification instructions.
