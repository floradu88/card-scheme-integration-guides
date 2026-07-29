# SLO, SLI, and Error Budget Plan

## Suggested SLIs

### Visa connectivity
- successful API requests / eligible requests;
- latency by operation;
- TLS failures;
- timeouts;
- 4xx and 5xx rates.

### Interchange engine
- successful evaluations;
- p50/p95/p99 latency;
- unmatched rate;
- fallback rate;
- decision persistence failures;
- active-version consistency.

### Clearing and reconciliation
- feed freshness;
- correlation success rate;
- actual mapping success rate;
- unresolved variance backlog;
- reconciliation completion time.

## Example SLOs

| Service | Example SLO |
|---|---|
| Authorization API availability | 99.95% monthly |
| Interchange engine availability | 99.99% monthly |
| p99 engine latency | < 5 ms |
| Configuration consistency | 100% active nodes on approved checksum |
| Settlement feed freshness | 99.9% within expected window |
| Reconciliation completion | 99% within agreed business window |

Final values must reflect contracts and risk appetite.

## Error budgets

Error budget:

```text
Allowed unavailability = 100% - SLO
```

Use error budget consumption to control release velocity:

- normal: continue planned releases;
- elevated: increase review and reduce risky changes;
- exhausted: freeze non-critical releases and prioritize reliability.
