# Mastercard interchange fees and dynamic per-country updates

Guide for modeling Mastercard interchange fees as versioned, country-aware configuration that can be updated without redeploying application code. Parallel to the Visa pack under [`../../visa/interchange-fees/`](../../visa/interchange-fees/).

| Document | Purpose |
|----------|---------|
| [`01-mastercard-interchange-fees-overview.md`](01-mastercard-interchange-fees-overview.md) | What Mastercard interchange fees are, fee axes, IFR caps, documentation boundary |
| [`02-dynamic-fees-per-country.md`](02-dynamic-fees-per-country.md) | Country-keyed packages, effective dating, update workflow |
| [`03-update-runbook.md`](03-update-runbook.md) | Operational runbook for ingesting and activating country fee updates |
| [`examples/`](examples/) | Illustrative Mastercard country fee samples (not production rate tables) |
| [`schemas/mastercard-country-fee-package.schema.json`](schemas/mastercard-country-fee-package.schema.json) | JSON Schema for a country fee package |

## Important boundary

Published Mastercard interchange schedules, domestic supplements, and participant-specific bulletins change over time and may require Mastercard Connect or authorized access. Examples in this folder are **illustrative**. Always archive the exact official source used for each release.

## Related packs

- Shared interchange engine: [`../../interchange/`](../../interchange/)
- Phase 07 dynamic interchange engine: [`../../platform/07-dynamic-interchange-engine/`](../../platform/07-dynamic-interchange-engine/)
- Mastercard adapter: [`../adapter/`](../adapter/)
- Visa twin pack (same model, `network: VISA`): [`../../visa/interchange-fees/`](../../visa/interchange-fees/)

## Status

- Added: 2026-08-12
- Covers Mastercard interchange fee axes, IFR context, country package model, and operational update/rollback runbook
- Samples are illustrative only
