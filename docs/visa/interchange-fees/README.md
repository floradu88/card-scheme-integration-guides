# Visa interchange fees and dynamic per-country updates

Guide for modeling Visa interchange fees as versioned, country-aware configuration that can be updated without redeploying application code.

| Document | Purpose |
|----------|---------|
| [`01-visa-interchange-fees-overview.md`](01-visa-interchange-fees-overview.md) | What Visa interchange fees are, fee axes, IFR caps, documentation boundary |
| [`02-dynamic-fees-per-country.md`](02-dynamic-fees-per-country.md) | Country-keyed packages, effective dating, update workflow |
| [`03-update-runbook.md`](03-update-runbook.md) | Operational runbook for ingesting and activating country fee updates |
| [`examples/`](examples/) | Illustrative Visa country fee samples (not production rate tables) |
| [`schemas/visa-country-fee-package.schema.json`](schemas/visa-country-fee-package.schema.json) | JSON Schema for a country fee package |

## Important boundary

Published Visa interchange schedules, domestic supplements, and participant-specific bulletins change over time and may require authorized Visa access. Examples in this folder are **illustrative**. Always archive the exact official source used for each release.

## Related packs

- Estimation-only option: [`../integration-options/D-interchange-estimation-only/`](../integration-options/D-interchange-estimation-only/)
- Reconciliation option: [`../integration-options/E-clearing-reconciliation-only/`](../integration-options/E-clearing-reconciliation-only/)
- Shared interchange engine: [`../../interchange/`](../../interchange/)
- Phase 07 dynamic interchange engine: [`../../platform/07-dynamic-interchange-engine/`](../../platform/07-dynamic-interchange-engine/)
- Visa adapter: [../adapter/](../adapter/)
- Official website references: [../official-website-references.md](../official-website-references.md)
- Mastercard twin pack (`network: MASTERCARD`): [`../../mastercard/interchange-fees/`](../../mastercard/interchange-fees/)

## Status

- Added: 2026-07-29
- Covers Visa interchange fee axes, IFR context, country package model, and operational update/rollback runbook
- Samples are illustrative only
