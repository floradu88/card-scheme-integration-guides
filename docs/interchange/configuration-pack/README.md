# Dynamic Interchange Configuration and Integration Pack

Generated: 2026-07-29

This pack provides a vendor-neutral architecture for understanding, configuring, importing, exporting, validating, and executing Visa and Mastercard interchange qualification rules.

## Contents

1. `01-business-overview.md`
2. `02-technical-architecture.md`
3. `03-configuration-model.md`
4. `04-import-export-and-versioning.md`
5. `05-performance-and-runtime.md`
6. `06-current-system-integration.md`
7. `07-testing-observability-and-rollout.md`
8. `08-official-references.md`
9. `schemas/interchange-config.schema.json`
10. `examples/interchange-config.sample.json`
11. `examples/interchange-config.sample.yaml`
12. `examples/interchange-rules-import.csv`
13. `sql/reference-schema.sql`
14. `api/openapi-fragment.yaml`

## Important boundary

Public Visa and Mastercard documents define the business framework, participant responsibilities, public rules, and published interchange schedules. Exact message-field mappings, rate identifiers, certification requirements, and some qualification logic can be proprietary, region-specific, or available only through Visa Online, Visa Business News, Mastercard Connect, and customer implementation manuals.

The proposed engine therefore supports:
- public published rules;
- organization-specific operational rules;
- private network configuration imported from authorized internal sources;
- effective dating and immutable version history;
- separate simulation and production activation.

## Recommended operating model

- Treat configuration as signed, versioned data—not source code.
- Import into a staging version.
- Validate syntax, semantics, overlaps, gaps, and rate math.
- Run simulation and regression tests.
- Approve through four-eyes workflow.
- Atomically activate a version.
- Retain the exact version used for every transaction.

## In this repository

- Interchange index: [`../README.md`](../README.md)
- Phase 2 engineering handbook: [`../phase2-engineering-handbook/`](../phase2-engineering-handbook/)
- Existing-project extension addon: [`../addons/existing-project-extension-analysis.md`](../addons/existing-project-extension-analysis.md)
- Visa options (D/E): [`../../visa/integration-options/`](../../visa/integration-options/)
- Visa country fees / dynamic updates: [`../../visa/interchange-fees/`](../../visa/interchange-fees/)
- Mastercard country fees / dynamic updates: [`../../mastercard/interchange-fees/`](../../mastercard/interchange-fees/)
- Mastercard adapter mirror: [`../../mastercard/adapter/`](../../mastercard/adapter/)
- Source archive: [`../../../archives/interchange_configuration_pack.zip`](../../../archives/interchange_configuration_pack.zip)
