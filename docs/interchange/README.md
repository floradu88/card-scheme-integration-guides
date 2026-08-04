# Interchange (Visa + Mastercard)

Vendor-neutral packs for configuring, validating, and executing interchange qualification rules across **Visa** and **Mastercard**.

| Pack | Path | Purpose |
|------|------|---------|
| Configuration pack | [`configuration-pack/`](configuration-pack/) | Business overview, config model, schemas, samples, SQL, OpenAPI fragment |
| Phase 2 engineering handbook | [`phase2-engineering-handbook/`](phase2-engineering-handbook/) | End-to-end blueprint: domain, adapters, engine, import/export, reconciliation, runbooks |
| Addons | [`addons/`](addons/) | Extension analyses that build on the core packs (existing-project rollout, etc.) |

### Network adapters inside the handbook

- Visa: [`phase2-engineering-handbook/04-network-integration/visa-adapter.md`](phase2-engineering-handbook/04-network-integration/visa-adapter.md) (also [`../visa/adapter/`](../visa/adapter/))
- Mastercard: [`phase2-engineering-handbook/04-network-integration/mastercard-adapter.md`](phase2-engineering-handbook/04-network-integration/mastercard-adapter.md) (also [`../mastercard/adapter/`](../mastercard/adapter/))

Visa country-fee updates: [`../visa/interchange-fees/`](../visa/interchange-fees/)

### Addons

- Existing project extension analysis: [`addons/existing-project-extension-analysis.md`](addons/existing-project-extension-analysis.md)

Original ZIPs: [`../../archives/interchange_configuration_pack.zip`](../../archives/interchange_configuration_pack.zip), [`../../archives/interchange_phase2_engineering_handbook.zip`](../../archives/interchange_phase2_engineering_handbook.zip)
