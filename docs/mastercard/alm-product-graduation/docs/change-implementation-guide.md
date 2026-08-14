# Change Implementation Guide

## 1. Scope

Add Mastercard ACS/ALM Product Graduation Plus to an existing card/product orchestration service so an eligible issued PAN can move from one approved Mastercard product/program to another without a PAN or BIN change.

## 2. Code areas to change

### Domain

Add:

```csharp
public sealed record NetworkProductMigration(
    string CardId,
    string SourceProgramId,
    string TargetProgramId,
    string Reason,
    string IdempotencyKey);

public enum ProductMigrationStatus
{
    Requested,
    Validated,
    Submitted,
    Completed,
    Rejected,
    Unknown,
    Reconciling,
    ManualReview
}
```

Keep network concepts such as Mastercard request DTOs out of the domain.

### Network abstraction

Add or extend:

```csharp
public interface ICardProductNetworkAdapter
{
    Task<NetworkProductMigrationResult> MoveProductAsync(
        NetworkProductMigration request,
        CancellationToken cancellationToken);

    Task<NetworkProductState> GetProductStateAsync(
        string cardId,
        CancellationToken cancellationToken);
}
```

Add `MastercardAlmAdapter`.

### Mastercard infrastructure module

Create:

```text
Infrastructure/Mastercard/Alm/
  MastercardAlmClient.cs
  MastercardAlmAdapter.cs
  MastercardAlmOptions.cs
  MastercardSigningHandler.cs
  MastercardContractMapper.cs
  MastercardErrorMapper.cs
  MastercardVerificationClient.cs
  Generated/
```

`Generated/` should contain the client/types generated from or pinned to the exact Mastercard ACS OpenAPI contract.

### Business/product mapping

Create a configuration/repository mapping:

```text
InternalProgramId
Network
MastercardProductCode
AllowedAccountRanges/BINs
UpgradeAllowed
DowngradeAllowed
EffectiveDateRules
Enabled
```

Do not permit a caller to supply arbitrary Mastercard product codes.

### Card repository

Existing card lookup must expose a secure card/PAN reference and the current network product/program state. Do not expand PAN exposure outside the PCI-controlled boundary just to support ALM.

### Migration persistence

Add a migration table/entity with:

```text
MigrationId
CardId
IdempotencyKey
SourceProgramId
TargetProgramId
SourceNetworkProduct
TargetNetworkProduct
BIN/account-range reference
PAN fingerprint/token reference
Status
Reason
MastercardRequestId/correlation id
FailureCode
CreatedUtc
SubmittedUtc
CompletedUtc
ReconciliationCount
LastReconciledUtc
```

Never store clear PAN in the migration/audit table.

## 3. Configuration changes

Add:

```json
{
  "MastercardAlm": {
    "Enabled": false,
    "BaseUrl": "...",
    "ApiVersion": "...",
    "ConsumerKeySecretName": "...",
    "SigningKeySecretName": "...",
    "RequestTimeoutSeconds": 10,
    "ReconciliationEnabled": true,
    "EnabledBins": [],
    "EnabledProductPairs": []
  }
}
```

The base URL, API version and authentication type must match the actual Mastercard project.

## 4. API changes

Recommended internal endpoint:

```http
POST /api/v1/cards/{cardId}/product-migrations
Idempotency-Key: <business-operation-id>
```

Do not expose clear PAN or raw Mastercard product codes to normal client channels.

Recommended result:

```json
{
  "migrationId": "...",
  "cardId": "...",
  "sourceProgramId": "...",
  "targetProgramId": "...",
  "status": "Completed"
}
```

## 5. Orchestration sequence

```text
1. Resolve CardId -> secure card/PAN reference.
2. Read current local programme.
3. Read/verify current Mastercard product state where supported.
4. Resolve target internal programme -> approved Mastercard product code.
5. Validate BIN/account-range enablement.
6. Validate allowed product pair.
7. Validate no active migration.
8. Reserve idempotency key.
9. Persist migration = Validated.
10. Submit ACS/ALM Product Graduation operation.
11. Capture Mastercard correlation/request identifiers.
12. Verify resulting Mastercard state.
13. Only after confirmation, update local programme.
14. Mark migration Completed.
15. Emit audit event and metrics.
```

## 6. Idempotency changes

Add a unique constraint on the business idempotency key.

Rules:

```text
same key + same payload + Completed -> return prior result
same key + same payload + Submitted/Reconciling -> return current status
same key + different payload -> 409
new key + already at target -> NoChange
```

Do not assume Mastercard itself provides the idempotency semantics your application requires.

## 7. Resilience changes

Avoid a generic `RetryPolicy` around the write operation.

Use:

```text
pre-send connection failure -> safe bounded retry may be possible
429 -> respect Mastercard response guidance / Retry-After where applicable
authentication failure -> no automatic retry
business validation failure -> no retry
5xx before outcome known -> reconciliation decision
timeout after request may have been transmitted -> Unknown -> reconcile
connection reset after write -> Unknown -> reconcile
```

## 8. Reconciliation worker

Add a scheduled worker for `Unknown` and `Reconciling` migrations.

```csharp
public interface IProductMigrationReconciliationService
{
    Task ReconcileAsync(Guid migrationId, CancellationToken ct);
}
```

Logic:

```text
query Mastercard effective state
    target product -> complete locally
    source product -> controlled resubmit only if approved/safe
    unexpected product -> ManualReview
    no authoritative state -> retry reconciliation with bounded attempts
```

## 9. Logging changes

Redact:

- PAN;
- Authorization header;
- signing material;
- full Mastercard payload when it contains account data.

Log:

- migration id;
- internal card id;
- masked PAN if policy permits;
- BIN/account-range reference;
- source/target internal programme;
- source/target network product;
- Mastercard correlation id;
- status;
- HTTP code;
- latency;
- reconciliation state.

## 10. Metrics changes

Add:

```text
mastercard_alm_requests_total
mastercard_alm_request_duration_ms
mastercard_alm_failures_total{category}
mastercard_alm_migrations_total{source,target,status}
mastercard_alm_unknown_outcomes_total
mastercard_alm_reconciliations_total{result}
mastercard_alm_state_mismatch_total
mastercard_alm_auth_failures_total
```

`state_mismatch_total > 0` should be treated as a high-severity operational signal.

## 11. Feature flags

Add:

```text
MastercardAlm.Enabled
MastercardAlm.EnabledBins
MastercardAlm.EnabledProductPairs
MastercardAlm.RolloutPercentage
```

Rollout should start with known certification/pilot accounts, not general traffic.

## 12. Database migration order

1. Add migration/audit tables.
2. Add product mapping/account-range configuration.
3. Add status indexes.
4. Add idempotency uniqueness.
5. Deploy code with feature disabled.
6. Validate DB backward compatibility.
7. Run Sandbox/MTF.
8. Enable pilot.
9. Expand gradually.

## 13. Existing-system behavior to review

Before enabling ALM, search the codebase for assumptions such as:

```text
product change => issue new card
product id immutable after activation
BIN uniquely determines product
one BIN maps to exactly one card product
authorization rules cached forever by card id
benefits/pricing derived only from issuance-time product
downstream ledger/card profile copied only during issuance
card artwork/benefits tied directly to PAN creation event
```

Every one of these assumptions can become wrong after same-PAN Product Graduation.

## 14. Downstream systems to update/revalidate

Review:

- card management system;
- authorization/risk rules;
- fee and pricing engine;
- interchange/product classification;
- loyalty/benefit eligibility;
- statements;
- customer servicing UI;
- notifications;
- CRM;
- data warehouse;
- fraud models;
- digital wallet metadata where relevant;
- tokenized-card experiences;
- dispute/chargeback metadata;
- reporting;
- regulatory/customer disclosures.

The network product change may be correct while downstream issuer systems still show the old product. That is a production defect.

## 15. Deployment

Use expand/contract:

```text
Release A: schema + disabled code
Release B: Sandbox/MTF verified adapter + reconciliation
Release C: pilot enabled
Release D: progressive rollout
Release E: remove legacy assumptions only after stabilization
```

Do not combine schema change, Mastercard network activation and broad customer rollout into one irreversible deployment.
