# Implementation Plan — Option D — Interchange Estimation Only

## Workstreams

### Business and governance
- Confirm scope and success measures.
- Assign business, scheme, engineering, finance, security, and operations owners.
- Establish source-document access and release approval.

### Connectivity and security
- Environment-specific endpoints.
- Mutual TLS.
- Certificate storage and rotation.
- Outbound firewall and DNS.
- Sensitive-data redaction.
- PCI-aware test data.

### Domain and adapters
- Keep Mastercard wire/API models separate from internal payment models.
- Map into a stable normalized transaction context.
- Version every parser and derivation.

### Configuration
- Use immutable, effective-dated packages.
- Import into draft.
- Validate schema, references, overlaps, gaps, tests, and source traceability.
- Promote and atomically activate.

### Testing
- Unit and mapping tests.
- Sandbox tests.
- Certification tests where applicable.
- Historical replay.
- Shadow comparison.
- Failure and rollback exercises.

### Operations
- Metrics, logs, traces, active-version health.
- Certificate alerts.
- Incident runbook.
- Reconciliation and support workflow.
