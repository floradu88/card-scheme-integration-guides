# Logging and Distributed Tracing

## Structured logging

Recommended fields:

```text
timestamp
severity
service
environment
trace_id
span_id
correlation_id
transaction_id_hash
network
operation
configuration_version
program_id
rule_id
result
reason_code
latency_ms
```

## Never log

- PAN;
- CVV/CVC;
- PIN or PIN block;
- full track data;
- cryptographic keys;
- private certificates;
- complete request/response payloads containing payment data;
- sensitive authentication data.

## Tracing

Use OpenTelemetry.

Suggested spans:

```text
payment.request
payment.validation
visa.request
visa.response.mapping
interchange.derive
interchange.evaluate
decision.persist
clearing.ingest
reconciliation.correlate
reconciliation.compare
```

## Sampling

- baseline probabilistic sampling;
- 100% for errors;
- 100% for certification environments if safe;
- targeted sampling for disputed transactions;
- no sensitive payload capture.
