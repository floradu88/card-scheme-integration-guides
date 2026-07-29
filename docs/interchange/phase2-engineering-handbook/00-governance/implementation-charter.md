# Implementation Charter

## Mission

Create a deterministic, explainable and versioned interchange engine that predicts or assigns a network interchange program without requiring application redeployment for ordinary scheme-rate changes.

## In scope

- Visa and Mastercard.
- Authorization-context estimation.
- Clearing-context final qualification or reconciliation.
- Effective-dated programs and rules.
- Network, region, product, MCC, channel, authentication and timing criteria.
- Merchant/acquirer overrides.
- Bulk import/export.
- Four-eyes activation.
- Historical replay.
- Expected-versus-actual variance.

## Out of scope unless explicitly added

- Merchant pricing/MDR engine.
- Scheme assessment fees.
- Chargeback fee calculation.
- Issuer cardholder pricing.
- Transaction routing optimization.
- Unauthorized extraction or redistribution of portal-only network documents.

## Decision rights

| Decision | Owner |
|---|---|
| Business interpretation | Scheme Operations + Finance |
| Production configuration approval | Scheme Operations + independent approver |
| Engine behavior | Architecture + Engineering |
| Source legitimacy | Compliance + Scheme Operations |
| Production activation | Operations under change control |
| Reconciliation tolerance | Finance |
| Emergency rollback | Operations incident commander |

## Definition of done

- Every result includes configuration version, rule, program, rate components and explanation.
- Historical decisions are reproducible.
- Ambiguous rule overlaps cannot be activated.
- Production changes require no application restart or deployment.
- Last-known-good configuration remains usable during control-plane failure.
- Actual network assessments can be reconciled to predictions.
