# Target Architecture

```text
Channels / Merchants
        |
        v
Payment Gateway / Orchestrator
        |
        +---------------------+
        |                     |
        v                     v
Visa Adapter             Other Network Adapters
        |
        v
VisaNet Connectivity
        |
        +----------------------+
        |                      |
        v                      v
Normalized Transaction     Payment Lifecycle Store
        |
        v
Compiled Interchange Engine
        |
        +----------------------+
        |                      |
        v                      v
Decision Store            Clearing / Settlement
                                 |
                                 v
                         Reconciliation Engine
```

## Control plane

- source registry;
- configuration import;
- validation;
- approval;
- simulation;
- version repository;
- activation;
- rollback.

## Data plane

- network adapters;
- normalized transaction context;
- local immutable configuration snapshot;
- deterministic rule evaluation;
- fee calculation;
- decision explanation;
- asynchronous persistence where safe.

## Architectural principles

- no database call in the interchange hot path;
- no remote configuration lookup in the hot path;
- network transport separated from domain;
- estimates separated from actuals;
- environment isolation;
- immutable configuration;
- atomic activation;
- strong auditability;
- last-known-good fallback.
