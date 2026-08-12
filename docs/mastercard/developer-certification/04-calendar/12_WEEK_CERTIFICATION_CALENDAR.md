# 12-Week Certification Calendar Template

> Planning baseline only. Mastercard states onboarding can range from roughly two weeks to multiple months. Rebaseline with the assigned Implementation Manager.

| Week | Engineering | Mastercard / External | Evidence / Gate |
|---|---|---|---|
| W1 | Confirm API/use case, architecture, owners | Request restricted access; contact Mastercard/acquirer | Scope + dependency register |
| W2 | Create project, config model, skeleton adapter | Review Onboarding Dashboard/Assets | Project inventory |
| W3 | mTLS + secrets + basic connectivity | Sandbox credentials/test data | Connectivity evidence |
| W4 | MLE + mappings + error model | Resolve product questions | Security test evidence |
| W5 | Authorization/core flow + correlatnId | Start commercialization/VDP track if not started | Functional suite |
| W6 | Capture/refund/void/etc. as in scope; HAL | Start Going Live Request | Sandbox exit report |
| W7 | Negative, timeout, idempotency, performance | Submit CSR/configuration information | Security/perf report |
| W8 | Certification environment deployment | Receive certification credentials/test data | Certification readiness gate |
| W9 | Execute certification cases | Sessions with Implementation Manager/testing team as required | Test evidence |
| W10 | Fix/retest; PCI/compliance evidence | Certification review | Zero critical defects |
| W11 | Production deployment rehearsal + DR/rollback | Production credential/configuration track | Production readiness review |
| W12 | Smoke test, controlled go-live, hypercare | Final approvals / production activation | GO decision |

## Recurring cadence
- Monday: engineering readiness review
- Tuesday: integration defects/evidence
- Wednesday: security/compliance review
- Thursday: external dependency/Mastercard/acquirer checkpoint
- Friday: RAID + certification evidence + next-week plan

## Long-lead items to start immediately
- restricted product approval
- acquirer/sponsor approval
- VDP agreement/commercialization
- PCI attestation
- CSR/certificate process
- production firewall/DNS/egress
- callback domain/certificate if applicable
