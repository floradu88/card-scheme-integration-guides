# Risks and Controls

| Risk | Control |
|---|---|
| Sandbox behavior treated as production truth | Certification and production-specific validation |
| Missing proprietary Visa mappings | Use authorized participant/acquirer material |
| Certificate expiry | Automated alerts and dual-certificate rollover |
| Duplicate financial transaction after retry | Operation-specific idempotency and reconciliation |
| Incorrect rate/effective date | Immutable versioning and source traceability |
| Configuration overlap | Static overlap and precedence validation |
| Sensitive data leakage | PCI-aware redaction and synthetic replay data |
| Node configuration drift | Active checksum health reporting |
| Wrong production activation | Four-eyes approval and atomic rollback |
| Actual-versus-estimated mismatch | Preserve both and classify variance |
