# Dynamic Interchange Configuration

## Business Goal
Support configurable interchange treatment during virtual card creation.

## Customer Journey
1. Client requests virtual card.
2. Client specifies interchange treatment.
3. Platform validates configuration.
4. Platform requests network treatment.
5. Network confirms.
6. Card created.
7. Card details returned.

## Phase 1
- Fixed configurable interchange levels
- BIN/sub-BIN matching
- REST API support
- Auditability

## Phase 2
- Dynamic rule engine
- Runtime configuration
- Multiple programs
- Advanced rule evaluation
