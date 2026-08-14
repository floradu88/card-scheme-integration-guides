# Mastercard ALM — Development & Developer Testing Estimate

## Context

Architecture and technical design are already completed.

The change is treated as an incremental addition to the existing Mastercard integration: add the required Account Catalog Services / Account Level Management Product Graduation API operation and integrate it into the existing client/adapter patterns.

Formal Mastercard certification/MTF, formal QA/UAT, production rollout, and external Mastercard waiting time are excluded.

Developer-owned unit, integration, contract, and failure-path testing are included in the development estimate.

## Initial detailed view

If substantial supporting behavior had to be created from scratch, the work could include:

- Mastercard ACS client/authentication/OpenAPI binding;
- ALM/Product Graduation implementation;
- product mapping and BIN/account-range eligibility;
- persistence/state management;
- idempotency;
- resulting-state verification;
- unknown-state/reconciliation handling;
- security/PAN redaction;
- logging/metrics/feature flags;
- developer tests and fixes.

That broader implementation would be approximately 18–22 MD, but it is not the recommended estimate when the surrounding architecture and Mastercard integration patterns already exist.

## Recommended incremental estimate

| Task | Estimate |
|---|---:|
| Add exact ACS endpoint/client method from OpenAPI | 0.5–1 MD |
| Request/response DTO mapping | 0.5 MD |
| Wire authentication/signing through existing Mastercard client | 0.5 MD |
| Add product/BIN validation | 0.5–1 MD |
| Persist/update migration result if required | 0.5–1 MD |
| Error handling + idempotency/retry behavior | 0.5–1 MD |
| Unit tests | 0.5–1 MD |
| Integration/Sandbox developer testing | 1–2 MD |
| Fixes/refinement | 0.5–1 MD |
| **Total** | **~5–8 MD** |

## Planning recommendation

> **Baseline: 6 MD**

> **Likely range: 5–8 MD**

This includes development and developer-owned testing.

## Best-case scenario

If the existing Mastercard integration already provides:

- authentication/signing;
- common HTTP client;
- error handling;
- correlation IDs;
- logging/redaction;
- resilience;
- OpenAPI/client-generation pattern;
- Sandbox configuration;
- product mapping;
- idempotency infrastructure;

then the implementation can realistically be:

> **3–5 MD**

## Why retain a 5–8 MD planning range?

The network request itself is small.

The additional effort protects the card-product mutation around:

```text
Product A
   |
   | Mastercard ACS / ALM API
   v
Product B

PAN = unchanged
BIN = unchanged
```

The main uncertainty is how much verification and recovery behavior is required.

### Simple implementation

If the requirement is:

```text
send request
-> receive response
-> map result
```

then:

> **3–5 MD**

is reasonable.

### Safer implementation

If the requirement also includes:

```text
send request
-> ambiguous timeout
-> determine whether Mastercard applied the change
-> verify effective state
-> avoid duplicate mutation
-> reconcile local state
```

then:

> **5–8 MD**

is the safer estimate.

## Suggested implementation sequence

### Day 1

- Bind exact Mastercard ACS operation from OpenAPI.
- Add request/response mapping.
- Reuse existing Mastercard authentication.
- Basic positive-path test.

### Day 2

- Integrate Product Graduation operation into existing service/adapter.
- Add source/target product mapping.
- Add BIN/account-range validation.
- Add error mapping.

### Day 3

- Persistence/state integration if required.
- Idempotency behavior.
- Unit tests.
- Negative-path tests.

### Day 4

- Mastercard Sandbox integration.
- Positive Product A -> Product B scenario.
- Validate same PAN / same BIN behavior.
- Error scenarios.

### Day 5

- Verification/reconciliation behavior where required.
- Timeout/failure scenarios.
- Fixes and regression testing.

### Days 6–8 — contingency

Use only where required for:

- Sandbox issues;
- Mastercard contract differences;
- unexpected issuer/product configuration;
- reconciliation;
- additional error handling;
- integration defects.

## Included

- implementation;
- developer unit tests;
- developer integration tests;
- contract tests where applicable;
- basic Sandbox developer verification;
- failure-path testing;
- implementation fixes/refactoring.

## Excluded

- architecture/design, because already completed;
- formal independent QA/UAT;
- Mastercard formal MTF/certification;
- Mastercard/issuer waiting time;
- production provisioning;
- production rollout;
- post-production monitoring period;
- unrelated downstream-system remediation.

## Final estimate

| Scenario | Estimate |
|---|---:|
| Existing integration is highly reusable | **3–5 MD** |
| Recommended planning baseline | **6 MD** |
| Normal likely range | **5–8 MD** |
| Unexpected integration/configuration complexity | **8+ MD** |

### Estimate to use

> **6 MD development + developer testing, with a 5–8 MD working range.**

Formal Mastercard certification and validation should be estimated separately.
