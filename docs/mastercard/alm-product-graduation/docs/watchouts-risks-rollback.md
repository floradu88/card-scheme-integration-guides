# Watch-outs, Risks, Rollback and Reconciliation

## Critical risks

### 1. Ambiguous network write

Most dangerous case:

```text
request leaves issuer
Mastercard applies migration
connection times out before response reaches issuer
issuer retries blindly
```

Treatment:

```text
status = Unknown
do not mutate local state
do not blindly resubmit
query/verify Mastercard
reconcile
```

### 2. Network/local divergence

Examples:

```text
Mastercard = Product B
Issuer DB = Product A
```

or:

```text
Mastercard = Product A
Issuer DB = Product B
```

Both require explicit detection, alerting and repair.

### 3. Assuming BIN defines product

Product Graduation Plus exists precisely because account-level treatment can differ while PAN/account range remains unchanged. Any downstream `BIN -> product` shortcut must be reviewed.

### 4. Unsupported product pair

A target product existing at Mastercard does not automatically mean a given source PAN/account range may graduate to it. Maintain an allow-list confirmed with issuer/Mastercard configuration.

### 5. Account range not enabled

Confirm whether the relevant account range must be enabled for Product Graduation Plus before PAN-level operations.

### 6. Stale caches

Purge or version caches that hold:

```text
card -> product
PAN token -> product
customer -> benefits tier
BIN -> assumed product
authorization rule set
pricing/interchange configuration
```

### 7. Downstream reissue triggers

Legacy systems may interpret a product change as requiring reissue. Ensure ALM migrations do not accidentally create:

- new PAN;
- replacement plastic;
- duplicate digital card;
- duplicate token provisioning;
- closure of the existing account.

### 8. Customer-facing inconsistency

Benefits, fees, name of product, servicing UI and communications must switch coherently with the effective network state.

### 9. Authentication/key failures

Mastercard OAuth signing/key rotation failures can produce broad outages. Test:

- key rotation;
- wrong alias;
- expired/revoked credential;
- clock skew if relevant;
- bad signature;
- secret-manager unavailable.

### 10. Logging leakage

HTTP tracing libraries may capture the request body automatically. Disable or redact bodies at this integration boundary.

## Rollback model

There are two different rollback types.

### Application rollback

Safe:

```text
disable feature flag
stop new migrations
keep reconciliation running
roll back application deployment if DB remains compatible
```

### Network product rollback

Not equivalent to deployment rollback.

Never assume you can undo:

```text
Product A -> Product B
```

by simply restoring the issuer database.

If Mastercard/network state is Product B, restoring local state to Product A creates divergence.

Network reversal must use an approved Product B -> Product A Product Graduation operation if that product pair/direction is supported and approved.

## Kill switch

Immediately stop new migrations if any occur:

```text
Mastercard effective-state mismatch
unexpected product code
PAN/BIN invariant violation
spike in Unknown outcomes
authentication failure across traffic
account-range rejection spike
duplicate product movements
security/PAN leakage concern
Mastercard requests suspension
```

The kill switch must not stop reconciliation of already submitted transactions.

## Operational thresholds

Define before launch, for example:

```text
state mismatch: zero tolerance
PAN/BIN invariant mismatch: zero tolerance
unknown outcome rate: low explicit threshold
auth failure rate: alert immediately if systemic
reconciliation age: alert if above agreed SLA
manual-review queue: alert on backlog
```

Use real thresholds agreed with operations rather than copying generic values.

## Production repair decision tree

```text
Migration failed before network submission
  -> safely fail locally

Migration explicitly rejected by Mastercard
  -> fail locally; no programme change

Migration explicitly accepted and verified
  -> complete locally

Timeout / reset / 5xx with uncertain write
  -> Unknown
  -> query Mastercard
      -> target state: complete locally
      -> source state: controlled retry if safe
      -> other state: ManualReview

Local update fails after Mastercard confirmed target
  -> do NOT reverse Mastercard automatically
  -> retry local commit / repair
  -> alert if not resolved

Downstream systems show old product
  -> keep Mastercard/local authoritative migration
  -> replay downstream product-changed event / repair consumers
```

## Compliance/security watch-outs

- PAN handling may change PCI scope.
- Clear PAN must stay within an approved PCI-controlled service boundary.
- Do not put PAN into metrics labels.
- Do not put PAN in distributed tracing baggage.
- Do not expose Mastercard product codes unnecessarily to customer channels.
- Treat signing private keys as high-value secrets.
- Record operator/actor and business reason for manual migration.
- Consider four-eyes approval for bulk/admin migrations.
- Add rate limits to administrative migration endpoints.
- Add anomaly detection for mass upgrades/downgrades.
