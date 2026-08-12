# Visa

Visa-specific integration packs extracted and organized in this repository.

| Pack | Path | Purpose |
|------|------|---------|
| Quick start | [`quick-start/`](quick-start/) | Visa Developer quick start and environment stage gates |
| Onboarding | [`onboarding/`](onboarding/) | Platform phase-02 Visa onboarding checklist |
| Adapter | [`adapter/`](adapter/) | Interchange Visa adapter notes |
| Interchange fees (per country) | [`interchange-fees/`](interchange-fees/) | Visa interchange fees and dynamic country fee updates |
| Integration options A–F | [`integration-options/`](integration-options/) | Scope choices from auth PoC through processor replacement |
| NFR, security & operations | [`nfr-security-operations/`](nfr-security-operations/) | Sandbox/cert path, NFRs, monitoring, security, DR, runbooks, stage gates |
| VisaNet Connect security (.NET) | [`visanet-connect-security/`](visanet-connect-security/) | mTLS, MLE, cert lifecycle, .NET patterns for VisaNet Connect Acceptance |

## Integration options

- [A — Authorization-only PoC](integration-options/A-authorization-only-poc/)
- [B — Authorization, capture, and clearing](integration-options/B-authorization-capture-clearing/)
- [C — Full payment processing](integration-options/C-full-payment-processing/)
- [D — Interchange estimation only](integration-options/D-interchange-estimation-only/)
- [E — Clearing reconciliation only](integration-options/E-clearing-reconciliation-only/)
- [F — Processor connection replacement](integration-options/F-processor-connection-replacement/)

## Related shared docs

- Platform Visa onboarding (canonical): [`../platform/02-network-integration/01-visa/`](../platform/02-network-integration/01-visa/)
- Phase 08 Visa developer & certification: [`../platform/08-visa-developer-certification/`](../platform/08-visa-developer-certification/)
- Interchange Visa adapter (canonical): [`../interchange/phase2-engineering-handbook/04-network-integration/visa-adapter.md`](../interchange/phase2-engineering-handbook/04-network-integration/visa-adapter.md)
- Interchange packs: [`../interchange/`](../interchange/)
- Dynamic country fees: [`interchange-fees/`](interchange-fees/)
- Original ZIPs: [`../../archives/`](../../archives/)
