# Interchange Phase 2 — Engineering Handbook

Generated: 2026-07-29

## Purpose

This package is a work-oriented blueprint for a configurable, high-performance Visa/Mastercard interchange qualification and reconciliation capability. It covers:

- business ownership and operating model;
- normalized payment domain;
- authorization, capture, clearing and settlement integration;
- configuration-as-data;
- fast deterministic rule evaluation;
- import/export, validation, approval and rollback;
- .NET reference implementation patterns;
- data model and APIs;
- reconciliation and downgrade diagnostics;
- admin UI;
- observability, testing, certification and runbooks;
- current official Visa and Mastercard public references.

## Critical boundary

Public network documents do **not** expose every proprietary program identifier, ISO field mapping, private bulletin, certification script, or regional qualification matrix. Production implementation must combine:

1. public network rules;
2. licensed/internal Visa and Mastercard implementation material;
3. processor/acquirer specifications;
4. actual clearing and settlement reports;
5. organization-specific commercial configuration.

The examples in this pack are illustrative and must not be treated as current production rate tables.

## Recommended reading order

1. `00-governance/implementation-charter.md`
2. `01-business/business-capability-and-value.md`
3. `02-domain/normalized-transaction-model.md`
4. `03-architecture/target-architecture.md`
5. `05-configuration/configuration-specification.md`
6. `06-engine/qualification-engine.md`
7. `07-dotnet-reference/implementation-guide.md`
8. `09-import-export/import-export-design.md`
9. `10-reconciliation/reconciliation-design.md`
10. `13-testing-certification/test-strategy.md`
11. `20-official-references/official-source-index.md`

## Fast implementation path

- Week 1–2: discovery, current-system field mapping, source inventory.
- Week 3–4: normalized transaction model and read-only configuration repository.
- Week 5–7: rule compiler, in-memory evaluator, import validation.
- Week 8–9: historical replay and reconciliation.
- Week 10–11: shadow production and operational dashboards.
- Week 12+: canary activation by network/region/product.

Actual duration depends on current-system access, scheme documentation access, certification scope, and source-data quality.

## In this repository

- Interchange index: [`../README.md`](../README.md)
- Configuration pack: [`../configuration-pack/`](../configuration-pack/)
- Existing-project extension addon: [`../addons/existing-project-extension-analysis.md`](../addons/existing-project-extension-analysis.md)
- Visa adapter mirror: [`../../visa/adapter/`](../../visa/adapter/)
- Visa country fees / dynamic updates: [`../../visa/interchange-fees/`](../../visa/interchange-fees/)
- Mastercard adapter mirror: [`../../mastercard/adapter/`](../../mastercard/adapter/)
- Platform network phase: [`../../platform/02-network-integration/`](../../platform/02-network-integration/)
- Source archive: [`../../../archives/interchange_phase2_engineering_handbook.zip`](../../../archives/interchange_phase2_engineering_handbook.zip)
