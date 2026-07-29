# Deployment and Rollout

## Components

- control-plane API;
- configuration repository;
- artifact/object store;
- publisher;
- runtime engine/library or sidecar;
- reconciliation worker;
- admin UI;
- monitoring.

## Rollout stages

1. Local and CI validation.
2. Development sample data.
3. UAT with authorized configuration.
4. Historical replay.
5. Shadow production.
6. Canary by network/region/product.
7. Read-only financial reporting.
8. Controlled downstream consumption.
9. Full rollout.

## Activation protocol

1. Package approved.
2. Nodes download and verify checksum.
3. Nodes compile and run smoke tests.
4. Readiness quorum reached.
5. Publisher sends activation epoch.
6. Nodes atomically switch.
7. Monitor.
8. Roll back on threshold breach.

## Database migration

Keep schema migration independent from ordinary rate-package activation. Configuration updates should not require database migrations.
