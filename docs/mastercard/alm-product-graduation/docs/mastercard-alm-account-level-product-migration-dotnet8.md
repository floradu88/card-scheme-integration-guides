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
