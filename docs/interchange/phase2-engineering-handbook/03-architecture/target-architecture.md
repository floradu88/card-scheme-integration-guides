# Target Architecture

```text
                  CONTROL PLANE
+---------------------------------------------------------+
| Source Registry -> Import -> Normalize -> Validate      |
| -> Simulate -> Approve -> Version Repository -> Publish |
+-----------------------------+---------------------------+
                              |
                     immutable snapshot
                              |
                  TRANSACTION DATA PLANE
+-----------------------------v---------------------------+
| Network Adapter -> Context Mapper -> Derived Attributes |
| -> Partition Lookup -> Compiled Rules -> Fee Calculator |
| -> Decision + Explanation                              |
+-----------------------------+---------------------------+
                              |
            +-----------------+------------------+
            |                                    |
     Decision Store                    Clearing Reconciliation
```

## Control plane

May use a database and service APIs. It can be slower because it is not in the payment hot path.

## Data plane

- local immutable snapshot;
- no control-plane dependency;
- no per-transaction database lookup;
- no arbitrary scripts;
- deterministic and thread-safe;
- instrumented with low-cardinality metrics.

## Deployment options

### Embedded library
Lowest latency. Appropriate for a single platform.

### Sidecar
Centralized packaging with local calls. Useful across heterogeneous services.

### Shared service
Simplest governance but introduces runtime network dependency. Use client-side cache/fallback.

### Batch
Useful for first delivery and reconciliation, not real-time authorization.

Recommended: central control plane + embedded or sidecar data plane.
