# Payment Platform Architecture Handbook v3

Scheme-agnostic platform handbook. Phase 02 covers **Visa** and **Mastercard** network integration; other phases apply to the whole payment platform.

| Phase | Path | Summary |
|-------|------|---------|
| 00 Master index | [`00-master-index/`](00-master-index/) | Stage checklist, phases overview, manifest |
| 01 Foundation | [`01-foundation/`](01-foundation/) | Domain, architecture, governance, stage gates |
| 02 Network integration | [`02-network-integration/`](02-network-integration/) | Visa & Mastercard onboarding, adapters, clearing, reconciliation, certification |
| 03 Security & compliance | [`03-security-compliance/`](03-security-compliance/) | PCI, GDPR, PSD2/SCA, 3DS, tokenization, threat model |
| 04 Platform engineering | [`04-platform-engineering/`](04-platform-engineering/) | Kubernetes, Terraform, CI/CD, observability, FinOps |
| 05 Operations | [`05-operations/`](05-operations/) | Incident, DR, runbooks, go-live, hypercare, BAU |
| 06 Engineering assets | [`06-engineering-assets/`](06-engineering-assets/) | Schemas, SQL, OpenAPI, templates, checklists |

### Scheme-specific entry points inside phase 02

- Visa: [`02-network-integration/01-visa/`](02-network-integration/01-visa/) (also [`../visa/onboarding/`](../visa/onboarding/))
- Mastercard: [`02-network-integration/02-mastercard/`](02-network-integration/02-mastercard/) (also [`../mastercard/onboarding/`](../mastercard/onboarding/))

Related deeper packs:

- Visa options & NFR: [`../visa/`](../visa/)
- Interchange (Visa + Mastercard): [`../interchange/`](../interchange/)
- Archives: [`../../archives/`](../../archives/)
