# Service Boundaries

## Mastercard connectivity service
Responsible for:

- mTLS;
- API request/response;
- endpoint configuration;
- operation-specific timeout and retry;
- Mastercard error mapping;
- request correlation.

## Payment orchestration
Responsible for:

- lifecycle state;
- idempotency;
- merchant configuration;
- routing;
- capture and reversal coordination.

## Interchange engine
Responsible for:

- qualification;
- rate calculation;
- explanation;
- version traceability;
- deterministic replay.

## Configuration control plane
Responsible for:

- import;
- schema and semantic validation;
- approval;
- package signing;
- activation;
- rollback;
- export and diff.

## Reconciliation
Responsible for:

- actual clearing/settlement ingestion;
- lifecycle correlation;
- expected-versus-actual variance;
- downgrade diagnostics;
- financial reporting.

## Monitoring platform
Responsible for:

- logs;
- metrics;
- traces;
- alerting;
- dashboards;
- SLO reporting.
