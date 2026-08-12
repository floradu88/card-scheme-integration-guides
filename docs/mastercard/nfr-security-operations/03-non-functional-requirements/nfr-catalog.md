# Non-Functional Requirements Catalog

## 1. Availability

### Objectives
- 24x7 operation for production payment paths.
- No single point of failure.
- Graceful degradation.
- Zero-downtime configuration activation.
- Last-known-good configuration available during control-plane outages.

### Requirements
- At least two runtime nodes.
- Load-balanced deployment.
- Readiness and liveness checks.
- Atomic configuration swap.
- Stateless application nodes where possible.
- Database high availability.
- Queue/topic redundancy where used.
- Documented dependency failure behavior.

## 2. Reliability

- Identical input and configuration version produce identical output.
- Rule order is deterministic.
- All financial decisions are reproducible.
- Duplicate requests are handled by operation-specific idempotency.
- Authorization estimates are never silently replaced by actuals.
- Partial failure must not create duplicate financial events.

## 3. Performance

Initial targets, subject to real capacity planning:

| Capability | Target |
|---|---:|
| In-process interchange p50 | < 1 ms |
| In-process interchange p95 | < 2 ms |
| In-process interchange p99 | < 5 ms |
| Local configuration lookup | O(1) partition lookup |
| Configuration activation | < 1 second after preload |
| Standard import validation | < 30 seconds |
| Replay throughput | >= 1 million transactions/hour |
| Mastercard API latency | tracked separately from local engine |

The local engine target excludes network latency.

## 4. Scalability

- Horizontal application scaling.
- Independent scaling for control plane and data plane.
- Partitioned rule candidates.
- Bulk replay workers.
- Multi-network, multi-region, multi-currency support.
- Support for growing merchant and BIN/product data.
- Capacity headroom defined before launch.

## 5. Maintainability

- Declarative rules.
- Stable schemas.
- Adapter isolation.
- Strong versioning.
- Feature flags.
- Backward-compatible APIs.
- Automated tests.
- documented deprecation process.

## 6. Auditability

Every configuration change records:

- who;
- when;
- source;
- old and new version;
- diff;
- reason;
- approval;
- checksum;
- activation target.

Every decision records:

- transaction reference;
- event time;
- configuration version;
- rule and program;
- rate;
- explanation;
- fallback indicator;
- input hash;
- engine version.

## 7. Recoverability

- Backups are encrypted.
- Restore is tested.
- RTO and RPO are explicit.
- Configuration packages are reproducible.
- Decision and audit stores can be reconstructed or restored.
- Emergency rollback is tested.

## 8. Compatibility

- Versioned Mastercard API contracts.
- Backward-compatible internal APIs.
- Contract tests.
- Explicit parser and mapping versions.
- Database migrations independent from rate changes.

## 9. Data quality

- Unknown values remain unknown.
- Required fields are validated.
- Data provenance is recorded.
- Mapping confidence is explicit.
- Missing data produces reason codes and metrics.
- No guessed proprietary mapping.

## 10. Cost efficiency

- Measure cloud cost by service and environment.
- Monitor replay/storage growth.
- Use retention and tiered storage.
- Avoid remote calls in local evaluation.
- Define budget alerts.
