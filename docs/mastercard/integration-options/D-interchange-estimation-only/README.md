# Option D — Interchange Estimation Only

Generated: 2026-07-29

## Goal

Estimate Mastercard interchange from existing authorization or transaction data without creating a new direct Mastercard transaction-processing connection.

## In scope

- Normalized transaction model
- Mastercard rate/program configuration
- BIN/product lookup
- Rule engine
- Authorization-time estimate
- Simulation and portfolio impact
- Import/export and versioning

## Out of scope

- Direct Mastercard authorization
- Clearing submission
- Production Mastercard connectivity
- Claiming final network-assessed interchange

## Architecture

```text
Existing Auth/Transaction Data -> Normalizer -> Local Compiled Rule Engine
                                               |              |
                                               |              +-> Versioned Config
                                               +-> Estimated Interchange Decision
```

## Milestones

1. Inventory existing transaction fields
2. Obtain authorized Mastercard rate/qualification sources
3. Build normalized context
4. Import configuration
5. Compile rule engine
6. Replay historical transactions
7. Expose estimate API

## Deliverables

- Interchange estimation API
- Rule-package schema
- CSV/JSON/YAML import examples
- Fast evaluation engine design
- Explainability model
- Historical replay plan

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
