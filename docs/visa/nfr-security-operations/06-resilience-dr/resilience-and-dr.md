# Resilience and Disaster Recovery

## Resilience principles

- fail safely;
- preserve financial consistency;
- avoid duplicate processing;
- use last-known-good configuration;
- isolate control-plane failure from data plane;
- make retries operation-specific;
- maintain clear recovery states.

## Dependency failure behavior

### Visa unavailable
- do not blindly retry financial operations;
- use operation-specific idempotency;
- preserve correlation;
- route to manual/recovery workflow where required;
- alert operations;
- reconcile later.

### Configuration service unavailable
- continue with local immutable snapshot;
- block new activation;
- alert on stale configuration age.

### Database unavailable
- define whether payment authorization can continue;
- buffer non-critical decision persistence if safe;
- never lose financial events silently;
- prevent duplicate commit after recovery.

### Queue unavailable
- local backpressure;
- bounded retry;
- dead-letter flow;
- operational alert.

## RTO and RPO template

| Capability | RTO | RPO | Strategy |
|---|---:|---:|---|
| Authorization runtime | business-defined | near-zero | active-active |
| Interchange engine | minutes or less | zero config loss | local snapshot |
| Decision store | business-defined | near-zero | HA + durable log |
| Configuration control plane | hours acceptable | zero approved package loss | DB/object-store backup |
| Reconciliation | business window | feed-dependent | replayable ingestion |

## Backup

- encrypted;
- monitored;
- immutable/offline where appropriate;
- cross-region copy;
- retention policy;
- restore test.

## DR exercises

At least:

- database restore;
- region failover;
- configuration repository restore;
- certificate recovery;
- queue recovery;
- settlement replay;
- complete operational simulation.

Record actual RTO/RPO achieved.
