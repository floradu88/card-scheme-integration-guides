# Nonfunctional Requirements

## Performance
- p95 engine evaluation target: <2 ms in process.
- p99 target: <5 ms.
- zero remote calls on evaluation path.
- bounded allocation and candidate count.
- bulk replay capability.

## Availability
- continue with last-known-good snapshot;
- atomic version switching;
- multi-node consistency checks;
- no partial activation.

## Auditability
- immutable package;
- checksums;
- source references;
- approval evidence;
- version used per decision;
- deterministic replay.

## Security
- least privilege;
- signed production package;
- secrets outside configuration;
- PCI-aware logging and test data;
- export authorization.

## Scalability
- horizontal nodes;
- precompiled predicates;
- partitioned candidates;
- compact BIN prefix lookup;
- asynchronous decision persistence when safe.

## Maintainability
- declarative operators;
- stable schema;
- network adapters separated from domain;
- test case bundled with each rule set.
