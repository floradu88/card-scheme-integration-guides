# Monitoring Plan

## Golden signals

- latency;
- traffic;
- errors;
- saturation.

Add payment-specific signals:

- authorization result;
- reversal rate;
- fallback;
- unmatched;
- reconciliation variance;
- settlement freshness;
- certificate health;
- configuration checksum.

## Infrastructure monitoring

Monitor:

- CPU;
- memory;
- disk;
- IOPS;
- network;
- container restarts;
- node health;
- thread pool;
- GC;
- database connections;
- queue lag;
- Kubernetes readiness;
- load balancer health.

## Application monitoring

- request rate;
- operation latency;
- exceptions;
- timeout count;
- retry count;
- dependency latency;
- circuit state;
- idempotency conflicts;
- serialization failures;
- parser failures.

## Visa connectivity

- TLS handshake failures;
- certificate expiry;
- 401/403;
- 429;
- 4xx;
- 5xx;
- timeouts;
- DNS failures;
- connection pool saturation;
- request/response correlation;
- Visa endpoint availability.

## Interchange monitoring

- evaluation count;
- latency histogram;
- candidate rules evaluated;
- match rate;
- fallback rate;
- unmatched rate;
- active configuration version;
- node checksum drift;
- import validation failures;
- activation and rollback count.

## Business monitoring

- authorization volume;
- approval and decline rate;
- capture rate;
- reversal rate;
- clearing volume;
- settlement amount;
- expected interchange;
- actual interchange;
- variance;
- unexpected downgrade;
- missing program code;
- missing settlement feed.

## Dashboards

### Executive
- availability;
- transaction volume;
- financial variance;
- incidents;
- certification/release status.

### Operations
- authorization and dependency health;
- backlog;
- failed messages;
- active configuration;
- certificate expiry.

### Finance
- expected vs actual;
- variance by network, country, product, merchant, MCC;
- downgrade rate;
- unresolved financial cases.

### Engineering
- latency;
- errors;
- CPU/memory;
- GC;
- database/queue health;
- deployment version;
- configuration checksum.

### Security
- authentication failures;
- privileged changes;
- secret access;
- certificate events;
- policy violations.
