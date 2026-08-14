# Mastercard

Mastercard-specific integration packs organized to mirror the Visa scheme layout. Shared interchange and platform packs remain dual-scheme (Visa + Mastercard).

| Pack | Path | Purpose |
|------|------|---------|
| Quick start | [`quick-start/`](quick-start/) | Mastercard Developers quick start and environment stage gates |
| Onboarding | [`onboarding/`](onboarding/) | Platform phase-02 Mastercard onboarding checklist |
| Adapter | [`adapter/`](adapter/) | Interchange Mastercard adapter notes |
| Interchange fees (per country) | [`interchange-fees/`](interchange-fees/) | Mastercard interchange fees and dynamic country fee updates |
| Integration options A–F | [`integration-options/`](integration-options/) | Scope choices from auth PoC through processor replacement |
| NFR, security & operations | [`nfr-security-operations/`](nfr-security-operations/) | Sandbox/cert path, NFRs, monitoring, security, DR, runbooks, stage gates |
| Mastercard Connect / Developers security (.NET) | [`mastercard-connect-security/`](mastercard-connect-security/) | OAuth 1.0a, payload encryption, cert lifecycle, .NET patterns |
| Developer & certification | [`developer-certification/`](developer-certification/) | Tooling, .NET plan, certification calendar (also platform phase 09) |
| ALM / Product Graduation Plus | [`alm-product-graduation/`](alm-product-graduation/) | Account Catalog Services (ACS) / Account Level Management — .NET 8 implementation, change guide, checklist, certification runbook; incremental estimate **6 MD** (5–8 MD) |
| Official website references | [`official-website-references.md`](official-website-references.md) | Curated Mastercard.com / Developers links and published facts |

## Integration options

- [A — Authorization-only PoC](integration-options/A-authorization-only-poc/)
- [B — Authorization, capture, and clearing](integration-options/B-authorization-capture-clearing/)
- [C — Full payment processing](integration-options/C-full-payment-processing/)
- [D — Interchange estimation only](integration-options/D-interchange-estimation-only/)
- [E — Clearing reconciliation only](integration-options/E-clearing-reconciliation-only/)
- [F — Processor connection replacement](integration-options/F-processor-connection-replacement/)

## Related shared docs

- Platform Mastercard onboarding (canonical): [`../platform/02-network-integration/02-mastercard/`](../platform/02-network-integration/02-mastercard/)
- Phase 09 Mastercard developer & certification: [`../platform/09-mastercard-developer-certification/`](../platform/09-mastercard-developer-certification/)
- ALM / Product Graduation Plus (ACS): [`alm-product-graduation/`](alm-product-graduation/)
- Interchange Mastercard adapter (canonical): [`../interchange/phase2-engineering-handbook/04-network-integration/mastercard-adapter.md`](../interchange/phase2-engineering-handbook/04-network-integration/mastercard-adapter.md)
- Interchange packs: [`../interchange/`](../interchange/)
- Dynamic country fees: [`interchange-fees/`](interchange-fees/)
- Visa twin packs: [`../visa/`](../visa/)
- Original ZIPs (Visa sources; Mastercard packs adapted in-repo): [`../../archives/`](../../archives/)
