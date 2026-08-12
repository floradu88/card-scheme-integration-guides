# Option B — Authorization, Capture, and Clearing

Generated: 2026-07-29

## Goal

Implement the payment lifecycle from authorization through capture and clearing, including lifecycle correlation and estimated-versus-final interchange context.

## In scope

- Authorization
- Capture
- Partial and multiple capture where supported
- Reversal and void
- Clearing integration
- Lifecycle correlation
- Clearing-time interchange reevaluation
- Settlement input mapping

## Out of scope

- Full acquirer platform replacement
- Merchant pricing
- Routing optimization
- Chargeback automation unless separately added

## Architecture

```text
Payment API -> Mastercard Adapter -> Mastercard Authorization/Capture
      |                              |
      +-> Lifecycle Store            +-> Clearing/Settlement Feed
                     \                    /
                      -> Correlation + Reconciliation
```

## Milestones

1. Complete Option A
2. Obtain full API-suite approval
3. Implement capture/reversal flows
4. Integrate clearing feed
5. Correlate authorization and presentment
6. Re-evaluate at clearing
7. Reconcile against network actuals

## Deliverables

- Lifecycle state machine
- Authorization-to-clearing sequence diagrams
- Correlation strategy
- Clearing field-mapping workbook template
- Reconciliation rules
- Certification scenario matrix

## Important implementation boundary

Public Mastercard documentation is not a complete substitute for authorized implementation manuals, regional qualification material, certification scripts, participant-specific configuration, settlement specifications, and processor/acquirer agreements.

## Recommended first action

Confirm the organization's role, acquiring sponsor, processor relationship, target region, required Mastercard APIs, and whether the objective is sandbox experimentation, certification, production processing, estimation, reconciliation, or migration.

## In this repository

- Options index: [`../README.md`](../README.md)
- Mastercard NFR / security / operations: [`../../nfr-security-operations/`](../../nfr-security-operations/)
- Platform network integration: [`../../../platform/02-network-integration/`](../../../platform/02-network-integration/)
- Interchange packs: [`../../../interchange/`](../../../interchange/)
- Mastercard country fees / dynamic updates: [`../../interchange-fees/`](../../interchange-fees/)
- Provenance: adapted in-repo from the Visa option twin (no separate Mastercard ZIP in `archives/`)
