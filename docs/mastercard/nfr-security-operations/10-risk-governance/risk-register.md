# Risk Register

| Risk | Impact | Likelihood | Primary mitigation |
|---|---|---|---|
| Client certificate expires | Critical | Medium | Rotation automation and alerts |
| Private key compromise | Critical | Low | Vault/HSM, access controls, revocation runbook |
| Wrong configuration | Critical | Low | Validation, simulation, four-eyes approval |
| Configuration drift | High | Medium | Checksum monitoring |
| Mastercard outage | High | Low/Medium | Dependency resilience and recovery workflow |
| Unsafe retry creates duplicate | Critical | Low | Operation-specific idempotency |
| Settlement mismatch | High | Medium | Automated reconciliation |
| Missing proprietary mappings | High | Medium | Authorized source registry |
| Sensitive data in logs | Critical | Low | Redaction and secure logging standards |
| Database outage | Critical | Low | HA, backups, failover |
| Queue backlog | High | Medium | Lag alerts and replay |
| BIN/product data stale | High | Medium | Version and freshness monitoring |
| Reconciliation correlation failure | High | Medium | Stable identifiers and fallback matching |
| Supply-chain compromise | Critical | Low | Signed artifacts and SBOM |
| Unauthorized package activation | Critical | Low | RBAC and separation of duties |
| DR not executable | Critical | Medium | Scheduled exercises |
| Cost growth | Medium | Medium | Cost observability and retention |
| Certification incomplete | High | Medium | Formal evidence checklist |
