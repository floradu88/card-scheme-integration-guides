# Visa

Visa-specific integration packs extracted and organized in this repository.

| Pack | Path | Purpose |
|------|------|---------|
| Onboarding | [`onboarding/`](onboarding/) | Platform phase-02 Visa onboarding checklist |
| Adapter | [`adapter/`](adapter/) | Interchange Visa adapter notes |
| Integration options A–F | [`integration-options/`](integration-options/) | Scope choices from auth PoC through processor replacement |
| NFR, security & operations | [`nfr-security-operations/`](nfr-security-operations/) | Sandbox/cert path, NFRs, monitoring, security, DR, runbooks |

## Integration options

- [A — Authorization-only PoC](integration-options/A-authorization-only-poc/)
- [B — Authorization, capture, and clearing](integration-options/B-authorization-capture-clearing/)
- [C — Full payment processing](integration-options/C-full-payment-processing/)
- [D — Interchange estimation only](integration-options/D-interchange-estimation-only/)
- [E — Clearing reconciliation only](integration-options/E-clearing-reconciliation-only/)
- [F — Processor connection replacement](integration-options/F-processor-connection-replacement/)

## Related shared docs

- Platform Visa onboarding (canonical): [`../platform/02-network-integration/01-visa/`](../platform/02-network-integration/01-visa/)
- Interchange Visa adapter (canonical): [`../interchange/phase2-engineering-handbook/04-network-integration/visa-adapter.md`](../interchange/phase2-engineering-handbook/04-network-integration/visa-adapter.md)
- Interchange packs: [`../interchange/`](../interchange/)
- Original ZIPs: [`../../archives/`](../../archives/)
