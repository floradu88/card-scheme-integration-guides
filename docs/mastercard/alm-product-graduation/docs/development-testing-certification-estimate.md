# Mastercard ALM / Product Graduation Plus
## Sample Development, Testing & Certification Estimate

> **Status:** Full greenfield / end-to-end project estimate. For incremental work on an existing Mastercard integration, prefer [`mastercard-alm-dev-and-testing-estimate.md`](mastercard-alm-dev-and-testing-estimate.md) (**6 MD baseline**, **5–8 MD** range).

> **Estimation type:** Ballpark / T-shirt-size implementation estimate  
> **Recommended baseline:** ~55 person-days  
> **Likely range:** 45–70 person-days  
> **Engineering duration:** ~6–8 weeks  
> **Expected calendar duration including Mastercard dependencies:** ~8–12 weeks  
> **Confidence:** Medium, pending exact Mastercard ACS contract, issuer entitlement, account-range configuration, and existing-system impact assessment.

## 1. Assumptions

This estimate assumes an existing .NET 8 platform, CI/CD, database and observability infrastructure, secure PAN/card lookup, and the ability to provision Mastercard ACS/ALM/Product Graduation Plus.

## 2. Detailed estimate

| Phase / Task | Development / Architecture | QA / Other |
|---|---:|---:|
| Discovery, entitlement, BIN/product confirmation, architecture | 3.5 MD | 2–4 MD |
| Mastercard API foundation: credentials, OAuth, OpenAPI client, errors | 6 MD | 3 MD |
| Core ALM implementation | 10.5 MD | 5.5 MD |
| Persistence, idempotency, state machine, reconciliation | 8 MD | 5 MD |
| Security, PCI, NFR, metrics, alerts, feature flags | 6.5–7.5 MD | 5–6 MD |
| Automated unit/contract/integration/failure tests | 9 MD | 3.5 MD |
| Mastercard Sandbox/MTF, evidence, fixes/retest | 7–9 MD | 7–9 MD |
| Production readiness, pilot and validation | 3.5 MD | 3.5 MD |

## 3. Overall planning estimate

| Area | Estimate |
|---|---:|
| Core development | 34–39 MD |
| Developer automated/integration testing | 8–10 MD |
| Mastercard Sandbox/MTF support & fixes | 7–10 MD |
| QA effort | 12–16 MD |
| Architecture / technical leadership | 4–6 MD |
| Security / PCI / DevOps participation | 4–7 MD |
| **Recommended project baseline** | **~55 MD** |
| **Likely range** | **45–70 MD** |

Role effort overlaps; do not add every line mechanically.

## 4. Timeline

```text
Week 1: entitlement, product/BIN confirmation, OpenAPI, architecture
Week 2: auth, generated client, DB, mappings
Week 3: Product Graduation, verification, idempotency, API
Week 4: reconciliation, failure handling, observability, PCI
Week 5: automated tests, Sandbox, defects
Week 6: MTF/certification scenarios and evidence
Week 7: certification fixes/retest and validation
Week 8: production provisioning, pilot, production validation
```

Recommended elapsed planning window: **8–12 weeks**, because Mastercard/issuer provisioning and validation are external dependencies.

## 5. Legacy-system impact

If the platform assumes `BIN -> Product`, or assumes a product change requires card reissue, add approximately **15–30 MD** for impact analysis, remediation and regression testing.

Review authorization, card management, fees, pricing/interchange, loyalty/benefits, statements, servicing, CRM, fraud/risk, reporting, warehouse, caches, wallet/token dependencies and disputes.

## 6. T-shirt size

| Scenario | Size | Estimate |
|---|---|---:|
| Clean account-level product model | M | 40–50 MD |
| Normal expected implementation | M/L | 50–60 MD |
| Several downstream changes | L | 60–70 MD |
| BIN-centric legacy remediation | XL | 70–90+ MD |

Recommended baseline: **M/L — 55 MD**.

## 7. Delivery gates

### Gate 1 — Technical implementation: ~35 MD

ACS client, authentication, Product Graduation, eligibility, persistence, idempotency, verification, reconciliation, security and observability.

### Gate 2 — Testing & Mastercard certification: ~15 MD

Automated tests, Sandbox/MTF, same-PAN/same-BIN evidence, certification evidence, fixes and retesting.

### Gate 3 — Production validation: ~5 MD

Production configuration, feature flags, pilot, monitoring, validation and handover.

## 8. Estimation checklist

- [ ] ACS entitlement confirmed.
- [ ] ALM / Product Graduation Plus confirmed.
- [ ] Account ranges/BINs confirmed.
- [ ] Source/target Mastercard product codes confirmed.
- [ ] Same-PAN migration confirmed.
- [ ] Same-BIN/account-range behavior confirmed.
- [ ] Exact ACS OpenAPI contract obtained.
- [ ] Authentication mechanism confirmed.
- [ ] Sandbox/MTF availability confirmed.
- [ ] Mastercard certification requirements confirmed.
- [ ] Existing BIN-to-product assumptions assessed.
- [ ] Card Management System impact assessed.
- [ ] Authorization impact assessed.
- [ ] Pricing/interchange impact assessed.
- [ ] Loyalty/benefits impact assessed.
- [ ] Fraud/risk impact assessed.
- [ ] Reporting/data impact assessed.
- [ ] PCI/security impact assessed.
- [ ] Production rollout model agreed.

## Final recommendation

> **55 person-days baseline**  
> **45–70 MD likely range**  
> **70–90+ MD if significant legacy/downstream remediation is required**  
> **6–8 weeks engineering execution**  
> **8–12 weeks elapsed including Mastercard validation**
