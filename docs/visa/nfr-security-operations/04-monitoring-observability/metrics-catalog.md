# Metrics Catalog

## Visa

```text
visa_requests_total{operation,status}
visa_request_duration_seconds{operation}
visa_tls_failures_total{reason}
visa_timeouts_total{operation}
visa_http_errors_total{status}
visa_certificate_expiry_days{certificate}
```

## Interchange

```text
interchange_evaluations_total{network,result}
interchange_evaluation_duration_seconds{network}
interchange_candidates_evaluated{partition}
interchange_matches_total{program}
interchange_fallback_total{network,region}
interchange_unmatched_total{network,region}
interchange_active_config_info{version,checksum}
interchange_config_activation_total{result}
```

## Reconciliation

```text
reconciliation_records_total{status}
reconciliation_variance_minor_units{network,currency}
reconciliation_unexpected_downgrade_total{reason}
reconciliation_correlation_failure_total{source}
settlement_feed_age_seconds{feed}
```

## Platform

```text
http_server_request_duration_seconds
process_cpu_seconds_total
process_resident_memory_bytes
dotnet_gc_collections_total
dotnet_thread_pool_queue_length
db_connection_pool_active
queue_consumer_lag
```

## Cardinality controls

Do not use high-cardinality labels such as:

- transaction ID;
- PAN/token;
- merchant ID at global scale;
- raw error text;
- configuration rule ID when millions exist.

Use logs or traces for high-cardinality investigation.
