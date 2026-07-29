# Testing and Certification Plan

## Unit tests

- mappings;
- operators;
- rate calculation;
- rounding;
- effective dates;
- precedence;
- idempotency rules;
- correlation logic;
- certificate-expiry logic.

## Integration tests

- Visa sandbox;
- mTLS;
- vault/certificate loading;
- database;
- queues;
- configuration activation;
- clearing ingestion;
- settlement mapping.

## Contract tests

- Visa request schema;
- Visa response schema;
- internal adapter contracts;
- clearing file formats;
- event schemas.

## Performance tests

- interchange p50/p95/p99;
- API latency;
- allocations;
- GC;
- connection pool;
- configuration activation under load;
- replay throughput;
- backlog recovery.

## Security tests

- SAST;
- DAST;
- dependency and container scanning;
- secret detection;
- access control;
- privilege escalation;
- penetration testing;
- TLS and certificate tests.

## Resilience tests

- dependency outage;
- node loss;
- database failover;
- queue delay;
- Visa timeout;
- certificate expiration;
- stale configuration;
- region failover.

## Historical replay

Use representative, tokenized data covering:

- products;
- countries;
- MCCs;
- channels;
- entry modes;
- tokenization;
- authentication;
- transaction lifecycle;
- edge cases;
- actual interchange outcomes.

## Certification

Use Visa/acquirer-provided:

- endpoints;
- credentials;
- test cards/data;
- scenarios;
- expected outcomes;
- evidence format;
- sign-off process.

Sandbox success is not certification.
