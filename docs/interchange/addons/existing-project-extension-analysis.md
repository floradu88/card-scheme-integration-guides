# Existing Project Extension Analysis
## Dynamic Interchange Capability for Single-Use Virtual Cards

**Document type:** Solution Architecture and Technical Analysis  
**Change type:** Extension of an existing production platform  
**Audience:** Product, Engineering, Architecture, Delivery, Operations, Security, Compliance, Finance  
**Naming:** Client and provider names have been replaced with generic terms.

---

## 1. Purpose of This Analysis

This document analyzes how a dynamic interchange capability can be introduced as an extension to an existing virtual-card issuing and processing platform.

The objective is not to design a new platform from scratch. The objective is to add the required capability while:

- preserving existing card-creation behavior;
- minimizing changes to stable production components;
- avoiding regression for existing Clients;
- reusing the current API hub, virtual-card services, processing platform, event pipeline, portal, and reporting mechanisms;
- introducing clear architectural boundaries that support later expansion;
- allowing a controlled transition from fixed interchange configuration to dynamic interchange selection.

The implementation should therefore follow an incremental extension model rather than a platform replacement or large-scale rewrite.

---

## 2. Existing Platform Assumptions

The analysis assumes that the current platform already contains most or all of the following capabilities:

- a Client-facing API hub;
- authentication and authorization for API consumers;
- virtual-card creation APIs;
- single-use virtual-card lifecycle management;
- PAN generation or allocation;
- BIN and sub-BIN configuration;
- integration with a card processor or payment network;
- transaction authorization and clearing flows;
- event or message generation;
- an operations portal;
- scheduled or daily reporting files;
- production monitoring and support processes;
- an existing relational database or equivalent transactional store.

These assumptions must be confirmed during discovery. Where a capability already exists, it should be extended rather than duplicated.

---

## 3. Change Impact Summary

The requested capability affects the following existing areas:

| Existing area | Required extension | Expected impact |
|---|---|---|
| Client API | Accept interchange selection during card creation | Medium |
| API validation | Validate supported rates and Client eligibility | Medium |
| Virtual-card workflow | Apply interchange before completing card creation | High |
| BIN allocation | Support rate-to-BIN or sub-BIN mapping | Medium in Phase 1 |
| Network integration | Send interchange treatment request | High in Phase 2 |
| Persistence | Store requested, assigned, confirmed, and settled values | Medium |
| Configuration | Add versioned interchange rules | High |
| Portal | Optionally display assigned interchange | Low to medium |
| Event messages | Optionally include applied interchange | Low to medium |
| Reporting files | Optionally include transaction-level interchange | Medium |
| Monitoring | Add technical and business telemetry | Medium |
| Reconciliation | Compare requested, confirmed, and settled results | High |
| Security and RBAC | Add configuration and operational permissions | Medium |
| Support model | Add runbooks and exception workflows | Medium |

---

## 4. Recommended Extension Strategy

The preferred approach is to add an **Interchange Orchestration module** to the existing card-creation workflow.

This module should initially be implemented as a well-isolated component inside the current solution, with explicit interfaces and ownership boundaries. It can later be extracted into a standalone service if scaling, release independence, ownership, or resilience requirements justify the split.

### Recommended evolution

```text
Stage 1
Existing Card Application
    ├── Existing Card Creation
    ├── Existing BIN Allocation
    └── New Interchange Module

Stage 2
Existing Card Application
    ├── Card Creation
    └── Interchange Orchestration Interface
             |
             v
    Independent Interchange Service
```

This provides a pragmatic path:

- avoid premature microservice extraction;
- keep the first delivery close to the existing transaction boundary;
- establish clean contracts from the beginning;
- preserve the option to extract later without redesigning the domain.

---

## 5. Existing Flow Versus Extended Flow

### 5.1 Existing assumed flow

```text
Client
  -> API Hub
  -> Virtual Card API
  -> Validate Request
  -> Select Card Product / BIN
  -> Create Virtual Card
  -> Return Card
```

### 5.2 Extended Phase 1 flow

```text
Client
  -> API Hub
  -> Virtual Card API
  -> Validate Existing Request
  -> Resolve Interchange Configuration
  -> Map Rate to BIN / Sub-BIN
  -> Execute Existing Card Creation
  -> Persist Interchange Assignment
  -> Return Card and Interchange Result
```

### 5.3 Extended Phase 2 flow

```text
Client
  -> API Hub
  -> Virtual Card API
  -> Validate Existing Request
  -> Validate Interchange Request
  -> Reserve Card / PAN Context
  -> Send Treatment Request to Payment Network
  -> Receive Confirmation
  -> Execute Existing Card Creation or Activation
  -> Persist Full Interchange Lifecycle
  -> Return Card and Confirmed Result
```

---

## 6. Components to Reuse

The extension should reuse the following existing components wherever technically suitable:

### API hub

Reuse for:

- routing;
- Client authentication;
- rate limiting;
- API versioning;
- correlation ID propagation;
- request logging and protection.

Only the API contract and applicable policies should be extended.

### Existing virtual-card API

Reuse the existing endpoint or introduce a backward-compatible version.

The preferred option is:

- preserve the current request model for existing Clients;
- introduce an optional `interchange` object;
- activate new behavior through Client entitlement and feature flags;
- avoid creating a separate card-creation API unless lifecycle semantics differ materially.

### Existing card-creation engine

The existing engine should remain responsible for:

- card creation;
- card lifecycle;
- limits;
- expiry;
- product controls;
- PAN or token generation;
- current card validations.

The new interchange module should orchestrate around this engine rather than duplicate it.

### Existing BIN allocation

Extend the existing allocation logic to consider:

- requested interchange;
- region;
- currency;
- Client agreement;
- card program;
- range availability;
- active configuration version.

### Existing network integration framework

Reuse existing:

- HTTP clients;
- certificate management;
- secure connectivity;
- retry libraries;
- telemetry;
- external error mapping;
- network-specific adapters.

A new interchange operation should be added behind the existing integration abstraction.

### Existing database

Add new tables or aggregates using controlled migrations.

Avoid storing interchange fields directly in unrelated card tables unless they are intrinsic to the existing card aggregate and frequently queried together.

### Existing event pipeline

Reuse current event infrastructure to publish:

- interchange requested;
- interchange confirmed;
- interchange rejected;
- card created;
- reconciliation exception.

### Existing operations portal

Extend current card and transaction screens rather than building a separate portal.

### Existing reporting framework

Add fields to existing event and file schemas using versioning and backward-compatible rollout.

---

## 7. New Components Required

Even in an extension model, several new capabilities are required.

### 7.1 Interchange orchestration module

Responsibilities:

- validate the requested rate;
- identify the active configuration;
- enforce Client and product eligibility;
- determine the Phase 1 range or Phase 2 network treatment;
- coordinate the existing card-creation engine;
- maintain state;
- expose a normalized result.

### 7.2 Interchange configuration module

Responsibilities:

- supported rate matrix;
- market and currency rules;
- Client-specific entitlements;
- BIN and sub-BIN mappings;
- network treatment codes;
- effective dating;
- approval state;
- configuration versioning;
- rollback.

### 7.3 Payment-network interchange adapter

Responsibilities:

- build network-specific interchange requests;
- secure and sign calls;
- normalize responses;
- support status lookup;
- preserve external idempotency references;
- classify retryable and non-retryable failures.

### 7.4 Reconciliation processor

Responsibilities:

- compare the original request with platform decisions;
- compare platform decisions with network confirmation;
- compare network confirmation with transaction and settlement results;
- create operational exceptions.

---

## 8. Changes to Existing API Contracts

The current API should be extended without forcing existing consumers to change immediately.

### Proposed optional request extension

```json
{
  "clientRequestId": "string",
  "card": {
    "type": "SINGLE_USE_VIRTUAL",
    "currency": "EUR",
    "country": "DE",
    "amountLimit": 1000.00
  },
  "interchange": {
    "requestedRate": 1.5,
    "rateUnit": "PERCENT"
  }
}
```

### Compatibility behavior

| Client state | Interchange object | Expected behavior |
|---|---|---|
| Existing Client, feature disabled | Missing | Current behavior |
| Existing Client, feature disabled | Present | Reject as unsupported or ignore only by explicit contract |
| Enabled Client | Missing | Reject or use configured default |
| Enabled Client | Present and valid | Apply requested treatment |
| Enabled Client | Present and invalid | Reject with explicit error |

The platform should not silently apply a different rate unless an approved fallback policy exists.

### Response extension

```json
{
  "cardRequestId": "string",
  "status": "COMPLETED",
  "card": {
    "cardId": "tokenized-reference",
    "maskedPan": "411111******1111"
  },
  "interchange": {
    "requestedRate": 1.5,
    "assignedRate": 1.5,
    "networkStatus": "CONFIRMED",
    "configurationVersion": "2026-08-01.3"
  }
}
```

Existing response fields must remain unchanged.

---

## 9. Changes to the Existing Domain Model

The current card model should not use a single interchange field to represent all lifecycle stages.

The following values must remain distinct:

- requested rate;
- validated rate;
- assigned rate;
- network-confirmed rate;
- transaction-reported rate;
- settled rate.

### Recommended new aggregate

`InterchangeInstruction`

Suggested fields:

- ID;
- card request ID;
- Client request ID;
- card ID or tokenized reference;
- requested rate;
- assigned rate;
- confirmed rate;
- settled rate;
- treatment code;
- configuration version;
- market;
- currency;
- BIN or sub-BIN reference;
- external instruction ID;
- status;
- retry count;
- failure code;
- created time;
- modified time;
- correlation ID.

This aggregate should reference the existing card request rather than replacing it.

---

## 10. Existing Database Extension

### Recommended schema additions

```text
InterchangeInstruction
InterchangeDecision
InterchangeNetworkAttempt
InterchangeConfiguration
InterchangeConfigurationVersion
InterchangeRule
InterchangeBinMapping
InterchangeAuditEvent
InterchangeReconciliationRecord
InterchangeOperationalException
```

### Migration approach

1. Add schema without activating functionality.
2. Deploy read and write support behind a feature flag.
3. Begin recording derived interchange values in shadow mode.
4. Validate data quality and performance.
5. Enable Phase 1 for a limited Client cohort.
6. Add network fields and state transitions for Phase 2.
7. Enable dynamic processing incrementally.

### Database concerns

- migration duration;
- locking on existing high-volume tables;
- new index write cost;
- retention and partitioning;
- reporting query impact;
- transaction-boundary changes;
- rollback compatibility.

Prefer additive migrations. Avoid destructive column changes during the rollout.

---

## 11. Transaction Boundary Analysis

The most important design decision is whether interchange confirmation and card creation must occur inside one logical transaction.

A distributed ACID transaction across the platform and payment network is not realistic. The extension should use a state machine with compensating and recovery behavior.

### Recommended state sequence

```text
RECEIVED
VALIDATED
INTERCHANGE_PENDING
INTERCHANGE_CONFIRMED
CARD_CREATION_PENDING
CARD_CREATED
COMPLETED
```

Failure states:

```text
REJECTED_VALIDATION
INTERCHANGE_REJECTED
INTERCHANGE_UNKNOWN
CARD_CREATION_FAILED
RECONCILIATION_EXCEPTION
```

### Required rule

A usable card must not be exposed to the Client until interchange reaches an acceptable confirmed state, unless an explicitly approved fallback mode is configured.

---

## 12. Idempotency Extension

The existing card API may already support request idempotency. That mechanism must be extended across the new network instruction.

The same idempotency context should cover:

- Client card request;
- interchange decision;
- external network request;
- card creation;
- final API response.

### Required behavior

If a Client retries after a timeout:

- return the existing request state;
- do not issue a second network instruction;
- do not create a second card;
- resume incomplete processing where safe.

A stable external reference should be generated before calling the network.

---

## 13. Phase 1 as an Extension

Phase 1 should be delivered primarily as a configuration and BIN-allocation extension.

### Required changes

- add supported interchange rates to configuration;
- map each supported rate to an eligible BIN or sub-BIN;
- extend allocation rules;
- store assigned rate and configuration version;
- expose the result in logs and operational records;
- reconcile rate-to-range selection.

### Avoid in Phase 1

- network API changes not required for fixed treatment;
- broad refactoring of the card engine;
- new standalone service unless necessary;
- changes to 3-D Secure or fraud monitoring;
- premature portal and reporting redesign.

### Phase 1 value

Phase 1 validates:

- commercial rules;
- Client API behavior;
- configuration governance;
- operational support;
- reconciliation;
- production volume assumptions.

It also prepares the domain and API model for Phase 2.

---

## 14. Phase 2 as an Extension

Phase 2 introduces a new external interaction into the existing card-creation path.

### Required changes

- reserve or identify the PAN context before final card completion;
- call the network interchange API;
- handle confirmation, rejection, timeout, and unknown state;
- update the card workflow state machine;
- support asynchronous processing where necessary;
- add operational recovery;
- add network-level reconciliation.

### Key integration question

The existing platform must determine whether:

- a PAN is generated before the network call;
- a PAN can be reserved but not activated;
- treatment is applied to a PAN, account, token, or program identifier;
- card creation can be safely resumed after network confirmation.

This determines where the new orchestration hook is inserted.

---

## 15. Feature Flags and Client Entitlements

The extension should be controlled by configuration, not code deployment alone.

Suggested flags:

- `DynamicInterchange.Enabled`
- `DynamicInterchange.Phase1Enabled`
- `DynamicInterchange.Phase2Enabled`
- `DynamicInterchange.AsyncMode`
- `DynamicInterchange.IncludeInEvents`
- `DynamicInterchange.IncludeInReports`
- `DynamicInterchange.PortalDisplay`
- `DynamicInterchange.FallbackEnabled`

Flags should be scoped by:

- environment;
- Client;
- market;
- currency;
- card program;
- traffic percentage.

---

## 16. Existing Portal Extension

The operations portal should be extended to show:

- requested rate;
- assigned rate;
- confirmed rate;
- final settlement rate where available;
- current interchange status;
- configuration version;
- external instruction reference;
- error or rejection reason;
- timeline of state transitions.

Access should be restricted through existing RBAC.

Sensitive card data must remain masked.

---

## 17. Existing Event and File Extension

### Event messages

Prefer schema evolution through optional fields:

```json
{
  "transactionId": "string",
  "cardId": "tokenized-reference",
  "interchange": {
    "requestedRate": 1.5,
    "confirmedRate": 1.5,
    "treatmentCode": "string"
  }
}
```

Existing consumers must continue to process messages without the new object.

### Daily files

Recommended additional columns:

- interchange request ID;
- requested rate;
- assigned rate;
- confirmed rate;
- settled rate;
- treatment code;
- configuration version;
- reconciliation status.

Use a new file version where adding columns could break positional consumers.

---

## 18. Existing Monitoring Extension

Add telemetry to the current monitoring platform.

### New metrics

- requests containing interchange;
- validation failures;
- allocation by rate;
- network confirmations;
- network rejections;
- timeouts;
- unknown states;
- card creation after confirmation;
- end-to-end latency;
- reconciliation mismatches;
- range exhaustion;
- configuration lookup failures.

### New trace spans

```text
ValidateInterchange
ResolveInterchangeConfiguration
AllocateInterchangeBin
SendNetworkTreatment
QueryNetworkTreatmentStatus
CreateVirtualCard
ReconcileInterchange
```

### New alerts

- confirmed interchange without card creation;
- card created without confirmed interchange;
- increase in network rejection rate;
- unknown status beyond threshold;
- missing active configuration;
- BIN or sub-BIN capacity below threshold;
- reconciliation mismatch spike.

---

## 19. Existing Security Model Extension

The current authentication approach should remain unchanged unless the network requires additional controls.

Add authorization scopes or permissions for:

- requesting dynamic interchange;
- viewing interchange details;
- editing configuration;
- approving configuration;
- resolving reconciliation exceptions.

Configuration changes should require maker-checker approval.

Every configuration update should record:

- previous value;
- new value;
- user identity;
- approval identity;
- timestamp;
- reason;
- effective date.

---

## 20. Backward Compatibility Analysis

Backward compatibility is a central requirement.

### API compatibility

- existing fields remain unchanged;
- new fields are optional for non-enabled Clients;
- new errors apply only to enabled flows;
- current API version remains usable;
- a new major version is required only if processing semantics become incompatible.

### Database compatibility

- additive schema changes;
- old application version can run during rolling deployment where possible;
- no immediate removal of legacy columns or logic.

### Event compatibility

- optional nested object;
- schema registry compatibility checks;
- consumer contract tests.

### Operational compatibility

- support teams retain current card search and investigation tools;
- new interchange details appear as extensions to existing screens and runbooks.

---

## 21. Deployment Approach for an Existing Production System

### Step 1 — Structural deployment

Deploy:

- schema additions;
- inactive code paths;
- configuration model;
- metrics;
- feature flags.

No Client behavior changes.

### Step 2 — Shadow evaluation

For selected traffic:

- derive the expected interchange decision;
- do not alter card creation;
- compare expected decision with existing BIN behavior;
- monitor latency and errors.

### Step 3 — Phase 1 pilot

Enable:

- one Client;
- one region;
- one currency;
- a limited rate set;
- restricted traffic.

### Step 4 — Phase 1 expansion

Expand by:

- rate;
- country;
- traffic;
- Client cohort.

### Step 5 — Phase 2 non-production certification

Validate network behavior and recovery scenarios.

### Step 6 — Phase 2 production pilot

Enable dynamic interchange for a small, controlled cohort.

### Step 7 — General availability

Enable only after:

- reconciliation is stable;
- operational runbooks are proven;
- latency meets target;
- no unresolved financial exceptions remain.

---

## 22. Codebase Integration Recommendations

### Suggested module structure

```text
src/
  Existing.VirtualCards.Api/
  Existing.VirtualCards.Application/
    CardCreation/
    Interchange/
      Commands/
      Queries/
      Validation/
      Orchestration/
      StateMachine/
  Existing.VirtualCards.Domain/
    Cards/
    Interchange/
  Existing.VirtualCards.Infrastructure/
    Persistence/
    PaymentNetwork/
    Configuration/
    Messaging/
  Existing.VirtualCards.Workers/
    Reconciliation/
    Recovery/
```

### Interface examples

```csharp
public interface IInterchangeDecisionService
{
    Task<InterchangeDecision> DecideAsync(
        InterchangeRequest request,
        CancellationToken cancellationToken);
}

public interface IInterchangeNetworkAdapter
{
    Task<NetworkInterchangeResult> ApplyAsync(
        NetworkInterchangeInstruction instruction,
        CancellationToken cancellationToken);

    Task<NetworkInterchangeStatus> GetStatusAsync(
        string externalReference,
        CancellationToken cancellationToken);
}

public interface IInterchangeConfigurationProvider
{
    Task<InterchangeConfigurationSnapshot> GetActiveAsync(
        InterchangeContext context,
        CancellationToken cancellationToken);
}
```

The existing card-creation handler should depend on these interfaces rather than network-specific implementations.

---

## 23. Testing Impact on the Existing Project

### Regression testing

Existing scenarios must continue to pass:

- card creation without interchange;
- existing Client authentication;
- existing card limits;
- existing BIN allocation;
- existing transaction authorization;
- existing reporting;
- existing portal searches.

### New test layers

- interchange rules;
- feature-flag behavior;
- Client entitlement behavior;
- duplicate request handling;
- partial failure recovery;
- event compatibility;
- report compatibility;
- database migration compatibility;
- rolling deployment compatibility.

### Mandatory production-like tests

- external timeout after network acceptance;
- application restart during processing;
- duplicate Client retry;
- database failover;
- queue replay;
- card creation failure after treatment confirmation;
- configuration activation during live traffic.

---

## 24. Delivery Backlog Structure

### Epic 1 — Discovery and architecture

- confirm current card-creation sequence;
- identify extension hook;
- confirm network API;
- define state machine;
- define API extension;
- define configuration model;
- produce architecture decision records.

### Epic 2 — Phase 1 configuration and allocation

- rate matrix;
- BIN mapping;
- allocation changes;
- persistence;
- feature flags;
- monitoring;
- reconciliation.

### Epic 3 — Dynamic interchange API

- request and response models;
- validation;
- Client entitlements;
- API documentation;
- contract testing.

### Epic 4 — Network integration

- adapter;
- authentication;
- treatment request;
- status lookup;
- error normalization;
- idempotency.

### Epic 5 — Workflow orchestration

- state machine;
- retry behavior;
- unknown-state recovery;
- card-creation integration;
- asynchronous mode.

### Epic 6 — Operations and reporting

- portal fields;
- events;
- files;
- reconciliation;
- dashboards;
- alerts;
- runbooks.

### Epic 7 — Security and compliance

- RBAC;
- audit;
- data classification;
- logging review;
- penetration testing;
- compliance sign-off.

### Epic 8 — Rollout

- shadow mode;
- pilot;
- canary;
- expansion;
- rollback validation.

---

## 25. Main Architectural Risks for the Existing Project

| Risk | Existing-project concern | Mitigation |
|---|---|---|
| Card-creation workflow is tightly coupled | Change may cause regressions | Introduce interface boundary and characterization tests |
| Existing API lacks idempotency | Duplicate cards or instructions | Add idempotency before enabling dynamic mode |
| BIN allocation is embedded in legacy logic | Difficult Phase 1 extension | Extract decision logic behind an allocation interface |
| Existing database has large card tables | Migration and lock risk | Add separate tables and additive migrations |
| Existing network client is synchronous only | Latency and timeout risk | Add asynchronous worker and status recovery |
| Existing events are rigid | Consumer breakage | Optional object or versioned schema |
| Existing support tools lack state visibility | Slow incident resolution | Add timeline and external reference to portal |
| Configuration is code-based | Slow and risky changes | Introduce versioned runtime configuration |
| Existing logging includes raw payloads | Data exposure risk | Redaction and structured safe logging |
| No settlement reconciliation exists | Financial disputes | Deliver reconciliation before broad rollout |

---

## 26. Recommended Architecture Decisions

The following decisions should be documented before development:

1. Extend the existing API or create a new version.
2. Insert interchange processing before card creation or before activation.
3. Keep orchestration in the monolith initially or deploy separately.
4. Use synchronous, asynchronous, or hybrid completion.
5. Store configuration in the existing database or dedicated store.
6. Use event-driven recovery or scheduled polling.
7. Use the current event schema or a new version.
8. Allow fallback rates or reject unavailable values.
9. Define the source of truth for the final applied rate.
10. Define whether Phase 1 and Phase 2 can run simultaneously by Client.

---

## 27. Recommended Implementation Position

For an existing project, the recommended initial position is:

- extend the existing card API;
- implement interchange as a new domain module;
- keep it inside the current deployment for Phase 1;
- define clean interfaces for later extraction;
- use separate persistence tables;
- introduce a durable state machine;
- add dynamic network integration in Phase 2;
- support feature flags at Client and market level;
- require reconciliation before broad rollout;
- avoid changes to unrelated authorization, 3-D Secure, and fraud systems.

This balances delivery speed, safety, operational simplicity, and future scalability.

---

## 28. Exit Criteria for the Existing Project Extension

The extension is production-ready when:

- existing Client flows are unchanged unless explicitly enabled;
- existing card-creation regression tests pass;
- enabled Clients can request an allowed interchange rate;
- unsupported rates are rejected deterministically;
- Phase 1 selects the correct configured BIN or sub-BIN;
- Phase 2 obtains a traceable network outcome;
- retries cannot create duplicate cards or instructions;
- partial failures are recoverable;
- operations can see the complete state timeline;
- reconciliation detects financial mismatches;
- configuration changes are versioned and approved;
- monitoring and alerting are active;
- rollback to existing behavior is tested;
- no Client or provider names are embedded in reusable code or documentation.

---

# Appendix A — Full Solution Architecture Plan

The detailed architecture and delivery plan follows below.

---

# Dynamic Interchange for Single-Use Virtual Cards
## Solution Architecture and Delivery Plan

**Document status:** Draft  
**Audience:** Product, Engineering, Architecture, Operations, Compliance, Finance, Delivery  
**Naming convention:** All organization and brand names have been intentionally replaced with generic terms.

---

## 1. Executive Summary

The Client is expanding into the online travel segment and requires configurable interchange treatment for single-use virtual cards.

The delivery is expected in two stages:

- **Phase 1:** support predefined interchange levels by allocating transactions to configured virtual-card BIN or sub-BIN ranges. Interchange values are configured in fixed increments of **0.5%** across defined UK and EU ranges.
- **Phase 2:** introduce **dynamic interchange**, allowing the Client to specify the required interchange value at the time a single-use virtual card is created.

The proposed architecture introduces a dedicated **Interchange Orchestration capability** between the Client-facing API layer, the card-processing platform, and the payment-network integration. The design prioritizes:

- deterministic rate application;
- strong auditability;
- idempotent card creation;
- controlled rollout;
- backward compatibility;
- operational traceability;
- configurable rate governance;
- future extension to additional products, countries, currencies, and network programs.

---

## 2. Business Context

The Client creates single-use virtual cards for travel-booking payments.

The Client wants to influence the interchange treatment applied to each issued card. In the initial rollout, only a limited set of interchange values will be available. Within the following four to six months, the solution should support more flexible, dynamically selected interchange rates.

### Expected business outcomes

- Increase competitiveness in the online travel segment.
- Allow the Client to select an appropriate commercial interchange treatment per card.
- Reduce manual operational intervention.
- Support differentiated pricing and commercial agreements.
- Improve traceability between the requested, assigned, applied, and settled interchange values.
- Create a reusable foundation for future dynamic card controls.

---

## 3. Current Requirement Summary

### 3.1 Phase 1

Implement predefined virtual-card BIN or sub-BIN ranges that map to interchange configurations in increments of **0.5%**.

The configuration applies to designated UK and EU BIN ranges.

This phase provides a limited set of supported interchange levels and acts as an initial controlled rollout.

### 3.2 Phase 2

Support dynamic interchange configuration at the time of single-use virtual-card creation.

The Client sends the requested interchange value through REST APIs exposed by the card-processing platform's API hub.

The card-processing platform requests application of the interchange treatment from the payment network before completing card creation.

### 3.3 Customer journey

1. The Client requests creation of a single-use virtual card.
2. The Client specifies the requested interchange rate in the API request.
3. The card-processing platform validates the request.
4. The card-processing platform requests interchange treatment from the payment network for the associated PAN or card token.
5. The payment network confirms or rejects the interchange treatment.
6. The card-processing platform creates or activates the single-use virtual card only after receiving an acceptable response.
7. The card-processing platform returns the card details and interchange outcome to the Client.
8. The Client shares the card details with its travel customer.
9. The travel customer uses the card to complete a booking.
10. Transaction, settlement, and reporting data are correlated with the original requested interchange value.

---

## 4. Scope

### 4.1 In scope

- REST API support for interchange selection during single-use virtual-card creation.
- Validation of requested interchange values.
- Mapping between requested interchange and eligible product, geography, currency, BIN, sub-BIN, and network program.
- Integration with the payment network for interchange-treatment application.
- Card creation only after successful or explicitly accepted interchange processing.
- End-to-end correlation and audit records.
- Configurable support for Phase 1 fixed increments and Phase 2 dynamic values.
- Operational monitoring, alerting, retry controls, and reconciliation.
- Controlled migration from fixed configurations to dynamic behavior.
- Security, authorization, and compliance controls.
- API documentation and consumer onboarding guidance.

### 4.2 Optional / nice to have

- Display the assigned interchange rate at card level in the operations portal.
- Include the applied interchange rate in transaction-level event messages.
- Include the applied interchange rate in daily transaction or settlement files.
- Provide downloadable reconciliation reports.
- Provide a Client-facing API to query the interchange outcome for a card.
- Support configurable approval rules for exceptional rates.
- Support simulation or quote mode before card creation.

### 4.3 Out of scope

- Changes to 3-D Secure.
- Changes to fraud transaction monitoring.
- Changes to unrelated card authorization logic.
- Changes to customer-facing booking flows, except API integration required to request the card.
- Retrospective modification of interchange after a card has been used, unless separately supported by the payment network.
- Manual overrides outside an approved operational workflow.

---

## 5. Key Architectural Principles

1. **Interchange is a governed configuration, not a free-form field.**
2. **The requested rate, assigned rate, network-confirmed rate, and settled rate must be stored separately.**
3. **Card creation must be idempotent.**
4. **No card should be returned as ready for use before interchange processing reaches an accepted state.**
5. **All external calls must be traceable using a shared correlation identifier.**
6. **Configuration changes must be versioned and auditable.**
7. **Phase 1 and Phase 2 must use the same core domain model.**
8. **Failures must be explicit; silent fallback to a different rate is not allowed unless contractually configured.**
9. **Operational recovery must not create duplicate cards or duplicate network instructions.**
10. **Sensitive card data must remain isolated from general application logs and analytics.**

---

## 6. Proposed Logical Architecture

```text
Client System
    |
    v
API Gateway / API Hub
    |
    v
Virtual Card API
    |
    +--> Authentication and Authorization
    +--> Request Validation
    +--> Idempotency Service
    +--> Interchange Orchestration Service
             |
             +--> Interchange Rules and Configuration Store
             +--> Eligibility and Pricing Rules
             +--> BIN / Sub-BIN Allocation Service
             +--> Payment Network Adapter
             +--> Audit and Event Store
             +--> Reconciliation Service
    |
    v
Card Issuing / Processing Platform
    |
    v
Payment Network
```

### 6.1 Main components

#### API Gateway / API Hub

Responsibilities:

- authenticate the Client;
- enforce quotas and rate limits;
- validate request schemas;
- propagate correlation IDs;
- protect downstream services;
- expose versioned APIs;
- provide access logs without storing sensitive card data.

#### Virtual Card API

Responsibilities:

- accept single-use virtual-card requests;
- validate business inputs;
- enforce idempotency;
- coordinate interchange application and card creation;
- return a deterministic outcome.

#### Interchange Orchestration Service

Responsibilities:

- resolve the requested interchange treatment;
- apply Phase 1 or Phase 2 rules;
- validate eligibility;
- determine the relevant BIN or sub-BIN;
- call the payment-network adapter;
- store state transitions;
- expose status and reconciliation data.

This service should be independently deployable even if it initially resides inside an existing application boundary.

#### Interchange Rules and Configuration Store

Stores:

- supported rates;
- country and region eligibility;
- currency eligibility;
- product and card-program eligibility;
- BIN and sub-BIN mappings;
- network program identifiers;
- effective dates;
- Client-specific commercial rules;
- fallback behavior;
- approval requirements;
- configuration version.

#### Payment Network Adapter

Responsibilities:

- encapsulate network-specific API contracts;
- map internal canonical messages to external requests;
- handle authentication, signing, certificates, and encryption;
- normalize responses and error codes;
- implement retry rules that respect external idempotency behavior;
- shield the core domain from network-specific changes.

#### Reconciliation Service

Responsibilities:

- compare requested, assigned, applied, and settled interchange;
- detect missing or mismatched outcomes;
- produce operational exceptions;
- support daily and intraday reconciliation;
- export data for finance and settlement teams.

---

## 7. Domain Model

### 7.1 Interchange request

Recommended fields:

```json
{
  "clientRequestId": "string",
  "interchange": {
    "requestedRate": 1.5,
    "rateUnit": "PERCENT",
    "treatmentCode": "optional-string"
  },
  "card": {
    "type": "SINGLE_USE_VIRTUAL",
    "currency": "EUR",
    "country": "DE",
    "amountLimit": 1000.00,
    "expiry": "2026-09"
  },
  "bookingReference": "string",
  "metadata": {
    "commercialAgreementId": "string"
  }
}
```

### 7.2 Interchange lifecycle record

Store at least:

- interchange request ID;
- Client request ID;
- card request ID;
- card ID or tokenized card reference;
- requested rate;
- validated rate;
- assigned rate;
- network-confirmed rate;
- settled rate, where available;
- rate unit;
- treatment code;
- BIN and sub-BIN;
- market and currency;
- rule/configuration version;
- request status;
- payment-network status;
- card-creation status;
- created and updated timestamps;
- correlation ID;
- failure code and failure category;
- retry count;
- source system;
- user or service identity that initiated the action.

### 7.3 Recommended statuses

```text
RECEIVED
VALIDATED
REJECTED_VALIDATION
INTERCHANGE_PENDING
INTERCHANGE_CONFIRMED
INTERCHANGE_REJECTED
CARD_CREATION_PENDING
CARD_CREATED
CARD_CREATION_FAILED
COMPLETED
RECONCILIATION_EXCEPTION
CANCELLED
```

---

## 8. API Design

### 8.1 Create single-use virtual card

```http
POST /v1/virtual-cards
Idempotency-Key: <unique-client-key>
X-Correlation-Id: <correlation-id>
```

The interchange object should be optional during migration but mandatory for Clients enabled for dynamic interchange.

### 8.2 Example successful response

```json
{
  "cardRequestId": "string",
  "status": "COMPLETED",
  "card": {
    "cardId": "tokenized-reference",
    "maskedPan": "411111******1111",
    "expiry": "09/26"
  },
  "interchange": {
    "requestedRate": 1.5,
    "assignedRate": 1.5,
    "networkStatus": "CONFIRMED",
    "configurationVersion": "2026-08-01.3"
  }
}
```

### 8.3 Asynchronous alternative

If network latency or operational constraints make synchronous completion unreliable:

```http
POST /v1/virtual-cards
```

returns:

```json
{
  "cardRequestId": "string",
  "status": "INTERCHANGE_PENDING",
  "statusUrl": "/v1/virtual-card-requests/{id}"
}
```

The Client then polls the status endpoint or receives a webhook.

### 8.4 API error categories

- `INVALID_INTERCHANGE_RATE`
- `UNSUPPORTED_MARKET`
- `UNSUPPORTED_CURRENCY`
- `UNSUPPORTED_CARD_PROGRAM`
- `RATE_NOT_ALLOWED_FOR_CLIENT`
- `NETWORK_REJECTED_TREATMENT`
- `NETWORK_TIMEOUT`
- `DUPLICATE_REQUEST_CONFLICT`
- `CARD_CREATION_FAILED`
- `CONFIGURATION_NOT_FOUND`
- `SERVICE_TEMPORARILY_UNAVAILABLE`

Each error should include:

- machine-readable code;
- safe message;
- retryable flag;
- correlation ID;
- optional remediation guidance.

---

## 9. Processing Flows

### 9.1 Phase 1 flow

1. Receive card request.
2. Resolve market, currency, product, and Client configuration.
3. Validate requested predefined rate.
4. Map the rate to an eligible BIN or sub-BIN.
5. Reserve or select the card range.
6. Create the card using the selected range.
7. Store the assigned interchange configuration.
8. Return the card and assigned rate.
9. Reconcile against network and settlement data.

### 9.2 Phase 2 synchronous flow

1. Receive card request with interchange data.
2. Validate authentication, authorization, schema, and idempotency.
3. Validate rate eligibility using the active configuration version.
4. Reserve a card reference or PAN allocation context.
5. Send the interchange-treatment instruction to the payment network.
6. Receive confirmation.
7. Create or activate the single-use virtual card.
8. Persist the full audit record.
9. Return the confirmed card and interchange outcome.
10. Publish internal events for reporting and monitoring.

### 9.3 Phase 2 asynchronous flow

Use this flow if external confirmation is not consistently fast enough for the API latency target.

1. Accept request and persist it.
2. Return `202 Accepted`.
3. Process the network instruction asynchronously.
4. Create the card after confirmation.
5. Publish completion or failure event.
6. Notify the Client through webhook or status polling.
7. Reconcile final transaction and settlement data.

### 9.4 Failure handling

#### Network timeout before confirmation

- do not assume failure;
- query status using the same external reference;
- retry only when the network contract confirms retries are safe;
- avoid creating the card until state is resolved.

#### Network confirms interchange, card creation fails

- retain the network confirmation;
- retry card creation idempotently;
- if unrecoverable, cancel or expire the interchange instruction where supported;
- raise a reconciliation exception.

#### Card created, response to Client lost

- the Client retries with the same idempotency key;
- return the original result;
- never issue a second card.

#### Requested rate unavailable

- reject explicitly, or
- apply a configured fallback only when the Client agreement permits it;
- return both requested and assigned values.

---

## 10. Configuration Model

A versioned configuration model is essential.

### 10.1 Example configuration

```yaml
clientId: generic-client
programId: travel-single-use
effectiveFrom: 2026-08-01T00:00:00Z
effectiveTo: null
markets:
  - region: EU
    countries: [DE, FR, IT, ES]
    currencies: [EUR]
    supportedRates: [0.5, 1.0, 1.5, 2.0]
    binMappings:
      "0.5": "BIN-RANGE-A"
      "1.0": "BIN-RANGE-B"
      "1.5": "BIN-RANGE-C"
      "2.0": "BIN-RANGE-D"
fallbackPolicy: REJECT
approvalPolicy:
  rateAbove: 2.0
  requiresApproval: true
```

### 10.2 Configuration capabilities

- effective dating;
- draft, approved, active, retired lifecycle;
- maker-checker approval;
- import and export;
- pre-deployment validation;
- conflict detection;
- rollback;
- version comparison;
- simulation against sample requests;
- immutable audit history.

---

## 11. Data and Persistence Design

Recommended logical tables or aggregates:

- `InterchangeRequest`
- `InterchangeDecision`
- `InterchangeNetworkInstruction`
- `VirtualCardRequest`
- `VirtualCardReference`
- `InterchangeConfiguration`
- `InterchangeConfigurationVersion`
- `InterchangeRule`
- `BinRangeMapping`
- `ClientEntitlement`
- `InterchangeAuditEvent`
- `ReconciliationRecord`
- `OperationalException`

### Data retention

Retention must align with:

- financial-record obligations;
- scheme requirements;
- contractual requirements;
- privacy obligations;
- incident investigation needs.

Sensitive PAN data should not be stored in the interchange domain unless strictly required. Prefer card tokens or internal card references.

---

## 12. Security and Access Control

### 12.1 API security

- OAuth 2.0 client credentials or mutual TLS.
- Fine-grained scopes such as:
  - `virtual-card:create`
  - `virtual-card:read`
  - `interchange:request`
  - `interchange:read`
- Request signing where required.
- Replay protection.
- IP allow-listing where contractually appropriate.
- Rate limiting and anomaly detection.

### 12.2 Internal authorization

Use role-based and, where needed, attribute-based access controls.

Suggested roles:

- Configuration Viewer
- Configuration Editor
- Configuration Approver
- Operations Analyst
- Reconciliation Analyst
- Support Investigator
- Security Auditor
- Platform Administrator

### 12.3 Sensitive data controls

- Never log full PAN, CVV, or secrets.
- Encrypt data in transit and at rest.
- Use managed secret storage.
- Rotate keys and certificates.
- Separate production access from development access.
- Record privileged access and configuration changes.
- Mask card data in portals and exports.

---

## 13. Compliance Considerations

The implementation should be assessed against:

- applicable payment-card security requirements;
- payment-network rules;
- issuer and processor obligations;
- financial record-retention obligations;
- privacy and data-protection requirements;
- outsourcing and third-party risk controls;
- audit and change-management standards.

A formal compliance review should confirm whether the new APIs, logs, data stores, exports, and operational portals expand the regulated cardholder-data environment.

---

## 14. Non-Functional Requirements

### 14.1 Availability

Suggested target:

- API availability: **99.95%** monthly, excluding agreed maintenance.
- No single point of failure in the interchange orchestration path.
- Multi-zone deployment for production components.

### 14.2 Performance

Initial target:

- internal validation: p95 below 150 ms;
- platform processing excluding payment-network latency: p95 below 500 ms;
- synchronous end-to-end card creation: target p95 below 3 seconds, subject to network capability.

Use asynchronous processing if the external dependency prevents a predictable synchronous SLA.

### 14.3 Scalability

- horizontally scalable stateless APIs;
- partitioning by Client, card program, or date where needed;
- queue-based buffering for external calls;
- independently scalable reconciliation workers;
- capacity tests at expected peak plus safety margin.

### 14.4 Reliability

- idempotent APIs;
- durable state transitions;
- transactional outbox for event publication;
- controlled retries;
- circuit breakers;
- dead-letter handling;
- reconciliation for uncertain outcomes.

### 14.5 Recovery

Suggested targets:

- RPO: 5 minutes or better;
- RTO: 60 minutes or better;
- documented replay and recovery procedures;
- tested backup restoration;
- tested recovery for partially completed requests.

---

## 15. Observability

### 15.1 Metrics

Business metrics:

- card requests by Client and market;
- requested rates by distribution;
- confirmed rates by distribution;
- rejection rate;
- fallback rate;
- card-creation success rate;
- average commercial value per rate;
- reconciliation mismatch count.

Technical metrics:

- API latency and error rate;
- payment-network latency;
- timeout rate;
- retry count;
- queue depth;
- circuit-breaker state;
- configuration-cache hit rate;
- reconciliation lag;
- duplicate request rate.

### 15.2 Logs

Use structured logs containing:

- correlation ID;
- Client request ID;
- card request ID;
- interchange request ID;
- masked card reference;
- configuration version;
- state transition;
- normalized external response code.

Do not include raw PAN, CVV, secrets, or full external payloads containing sensitive data.

### 15.3 Distributed tracing

Trace:

```text
Client
 -> API Gateway
 -> Virtual Card API
 -> Interchange Orchestration
 -> Payment Network Adapter
 -> Card Processing Platform
 -> Event / Reconciliation Pipeline
```

### 15.4 Alerts

Critical alerts:

- sustained network rejection spike;
- timeout spike;
- card created without confirmed interchange state;
- confirmed interchange without card creation beyond threshold;
- reconciliation mismatches;
- invalid or missing active configuration;
- unexpected rate distribution;
- certificate or credential expiry;
- dead-letter queue growth.

---

## 16. Reconciliation and Financial Control

Reconciliation is a first-class capability, not a reporting afterthought.

### Reconciliation levels

1. **Request reconciliation**  
   Client requested rate versus platform-validated rate.

2. **Assignment reconciliation**  
   Platform-assigned rate versus BIN or sub-BIN configuration.

3. **Network reconciliation**  
   Platform instruction versus network confirmation.

4. **Transaction reconciliation**  
   Network-confirmed rate versus transaction event data.

5. **Settlement reconciliation**  
   Expected rate versus settlement or clearing outcome.

### Exception examples

- requested and assigned rates differ without an approved fallback;
- confirmed rate is missing;
- transaction uses a card without a completed interchange record;
- settled rate differs from confirmed rate;
- duplicate network instruction;
- card created after network rejection;
- configuration version cannot be reconstructed.

---

## 17. Delivery Plan

## Phase 0 — Discovery and Contract Validation

**Objective:** remove ambiguity before implementation.

Activities:

- confirm supported rate units and precision;
- confirm whether 0.5% increments mean percentage points;
- confirm allowed minimum and maximum values;
- confirm eligible UK and EU markets;
- confirm BIN and sub-BIN ownership and capacity;
- confirm payment-network API semantics;
- confirm whether interchange application is synchronous;
- confirm idempotency and status-query capabilities;
- confirm cancellation behavior;
- confirm reporting and settlement data availability;
- define Client fallback expectations;
- define operational ownership;
- perform compliance and security assessment.

Deliverables:

- approved requirements;
- sequence diagrams;
- external API contract;
- canonical domain model;
- failure matrix;
- architecture decision records;
- test and certification plan.

---

## Phase 1 — Fixed Interchange by BIN / Sub-BIN

**Objective:** deliver the initial limited set of interchange levels.

Activities:

- define supported rate matrix;
- configure UK and EU BIN or sub-BIN mappings;
- implement rate validation;
- implement deterministic range allocation;
- persist assigned interchange metadata;
- expose rate in internal operational views;
- add logging, metrics, and reconciliation;
- test range exhaustion and fallback behavior;
- enable feature flags per Client and market.

Exit criteria:

- every issued card has a reconstructable rate assignment;
- no card can be created using an unsupported rate;
- configuration changes are approved and audited;
- reconciliation confirms correct rate-to-range mapping;
- rollback is tested.

---

## Phase 2 — Dynamic Interchange

**Objective:** allow the Client to select interchange at card-creation time.

Activities:

- extend the API contract;
- implement the orchestration state machine;
- build the payment-network adapter;
- implement external idempotency and status recovery;
- add synchronous or asynchronous completion model;
- implement Client entitlement rules;
- add advanced reconciliation;
- expose status-query API;
- optionally add webhooks;
- certify with the network and processing platform.

Exit criteria:

- requested, assigned, confirmed, and settled rates are independently traceable;
- failure recovery does not create duplicates;
- network uncertainty can be resolved operationally;
- SLA and volume tests pass;
- production support runbooks are approved.

---

## Phase 3 — Reporting and Operational Enhancements

Potential enhancements:

- card-level portal display;
- transaction event enrichment;
- daily file enrichment;
- Client reconciliation reports;
- self-service configuration requests;
- simulation or pricing quote API;
- rate analytics;
- anomaly detection;
- automated commercial-rule validation.

---

## 18. Migration Strategy

### 18.1 Backward compatibility

- retain the existing card-creation contract during migration;
- make the interchange field optional for non-enabled Clients;
- use Client and market feature flags;
- default to the existing behavior only for explicitly configured legacy flows;
- do not infer a dynamic rate from missing data.

### 18.2 Rollout model

Recommended sequence:

1. internal test Client;
2. non-production certification;
3. production shadow mode;
4. one market and one rate;
5. limited Client traffic;
6. expanded rate set;
7. expanded country coverage;
8. full dynamic capability.

### 18.3 Shadow mode

Before enabling dynamic behavior:

- accept or derive the requested rate;
- evaluate rules;
- call a simulator or non-impacting validation endpoint where available;
- compare expected results with current processing;
- do not alter live card issuance;
- measure mismatch and rejection rates.

---

## 19. Testing Strategy

### 19.1 Unit tests

- rate validation;
- precision and rounding;
- configuration selection;
- eligibility;
- fallback rules;
- state transitions;
- idempotency behavior.

### 19.2 Contract tests

- Client-to-platform API;
- platform-to-network API;
- event-message schemas;
- reporting file formats.

### 19.3 Integration tests

- successful interchange confirmation;
- validation rejection;
- network rejection;
- timeout and later confirmation;
- duplicate request;
- card-creation failure after confirmation;
- configuration change during request processing.

### 19.4 End-to-end tests

- card creation through booking transaction;
- transaction event enrichment;
- settlement reconciliation;
- operational support investigation.

### 19.5 Non-functional tests

- peak load;
- soak test;
- failover;
- network latency and outage simulation;
- database recovery;
- queue replay;
- certificate rotation;
- authorization and penetration testing.

---

## 20. Deployment and DevOps

Recommended deployment practices:

- infrastructure as code;
- separate environments;
- immutable artifacts;
- automated database migrations;
- feature flags;
- canary releases;
- automated rollback;
- configuration promotion with approval;
- secrets from managed secret storage;
- release evidence retained for audit.

### CI/CD quality gates

- build and unit tests;
- static analysis;
- dependency and container scanning;
- API contract tests;
- infrastructure policy checks;
- migration validation;
- security tests;
- performance smoke tests;
- deployment approval for production.

---

## 21. Operational Runbooks

Required runbooks:

- payment-network outage;
- high network rejection rate;
- uncertain interchange status;
- duplicate Client request;
- confirmed interchange but failed card creation;
- card created but Client timed out;
- reconciliation mismatch;
- invalid active configuration;
- BIN or sub-BIN exhaustion;
- credential or certificate expiry;
- rollback from dynamic to fixed processing.

Each runbook should define:

- detection;
- severity;
- immediate containment;
- diagnostic queries;
- retry or recovery action;
- escalation path;
- Client communication;
- post-incident review requirements.

---

## 22. Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| External API does not provide strong idempotency | Duplicate instructions | Internal idempotency, stable external reference, status checks |
| Network response is slow | Poor API latency | Asynchronous mode, polling or webhook |
| Requested rate is commercially invalid | Financial loss | Versioned entitlement rules and maker-checker approval |
| BIN range capacity is exhausted | Card-creation failure | Capacity monitoring, reserve thresholds, alternative ranges |
| Silent fallback changes commercial outcome | Client dispute | Explicit fallback policy and response transparency |
| Settlement differs from confirmation | Financial mismatch | Daily reconciliation and exception workflow |
| Configuration changes during processing | Inconsistent decisions | Snapshot configuration version at request time |
| Sensitive data leaks into logs | Compliance incident | Structured redaction and tokenized references |
| Partial failure leaves orphan state | Operational complexity | State machine, durable events, recovery jobs |
| Phase 1 design cannot evolve | Rework | Shared canonical domain model and orchestration boundary |

---

## 23. Architecture Decisions to Confirm

1. Synchronous versus asynchronous card creation.
2. Whether the payment network requires a PAN before interchange treatment can be requested.
3. Whether a PAN can be reserved without making it usable.
4. Whether the requested rate is a percentage, basis-point value, or treatment code.
5. Maximum decimal precision.
6. Whether the network can reject a valid configured rate based on transaction or merchant context.
7. Whether interchange can be changed before first use.
8. Whether treatment expires with the card.
9. Whether settlement files expose the final applied rate.
10. Whether multiple network providers must be supported.
11. Whether a fallback rate is allowed.
12. Whether card creation may continue when the network is unavailable.
13. Whether the Client requires immediate card details or accepts asynchronous completion.
14. Whether the operations portal and files are mandatory for the initial release.

---

## 24. Recommended Minimum Viable Production Scope

The minimum production-ready solution should include:

- versioned REST API;
- idempotency;
- Client entitlement validation;
- fixed and dynamic rate model;
- configuration versioning;
- network adapter;
- explicit state machine;
- correlation IDs;
- audit history;
- metrics and alerts;
- reconciliation;
- feature flags;
- retry and recovery controls;
- production runbooks;
- security and compliance sign-off.

Portal display and file enrichment may follow after the core issuance and reconciliation path is stable.

---

## 25. Definition of Done

The capability is complete when:

- the Client can request an allowed interchange rate;
- the platform validates the rate against the correct active rules;
- the payment network confirms or explicitly rejects the treatment;
- the card is created exactly once;
- the API returns an unambiguous outcome;
- every state transition is auditable;
- operations can recover uncertain and failed requests;
- finance can reconcile requested, confirmed, and settled values;
- unauthorized or unsupported rates are blocked;
- production monitoring and alerting are active;
- rollback and disaster-recovery procedures have been tested;
- no Client or brand-specific names are embedded in reusable architecture, code, or documentation.

---

## 26. Suggested Immediate Next Steps

1. Validate the payment-network API and certification requirements.
2. Confirm the precise commercial meaning of the interchange field.
3. Agree the synchronous or asynchronous customer experience.
4. Define the Phase 1 UK and EU rate matrix.
5. Design the canonical API and state model.
6. Build a thin network-adapter proof of concept.
7. Validate end-to-end idempotency.
8. Define reconciliation sources and exception ownership.
9. Complete security and compliance review.
10. Produce an implementation backlog split by Phase 1, Phase 2, and operational enhancements.
