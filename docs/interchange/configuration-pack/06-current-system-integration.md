# 6. Integration with the Current System

## 6.1 Discovery checklist

Map the existing platform:

- authorization entry points;
- capture/clearing entry points;
- message formats and network adapters;
- merchant and terminal configuration;
- card/BIN/product lookup;
- settlement and reconciliation feeds;
- fee ledger/accounting;
- configuration management;
- deployment model;
- observability stack;
- current rules embedded in code, database procedures or processor configuration.

## 6.2 Minimal integration pattern

```text
Existing transaction pipeline
  -> Context Mapper
  -> Interchange Qualification API/Library
  -> Decision
  -> Persist version/rule/program/amount
  -> Existing clearing and reporting flows
```

Start in shadow mode. Do not initially use the predicted result to alter financial postings.

## 6.3 Integration modes

### Embedded library

Best latency and no runtime network call. Requires coordinated deployment of library binaries, but configuration remains external and hot-reloadable.

### Internal service

Best for centralized governance and multiple platforms. Add local cache or sidecar to avoid a network dependency in authorization.

### Batch/reconciliation engine

Best first step when only clearing files are available. Lower operational risk, but not suitable for real-time estimation.

A hybrid model is usually strongest: shared engine library + central configuration service + batch reconciliation.

## 6.4 APIs

Recommended endpoints:

- `POST /v1/interchange/evaluate`
- `POST /v1/interchange/simulate`
- `POST /v1/config/packages/import`
- `POST /v1/config/packages/{id}/validate`
- `POST /v1/config/packages/{id}/approve`
- `POST /v1/config/packages/{id}/activate`
- `GET /v1/config/active`
- `GET /v1/config/packages/{id}/export`
- `GET /v1/config/diff`
- `POST /v1/interchange/replay`

## 6.5 Decision response

```json
{
  "decision_id": "uuid",
  "configuration_version": "mc-eu-2026.07.1",
  "program_id": "MC-EU-CONSUMER-CREDIT-CP",
  "rule_id": "rule-10024",
  "estimated_fee": {
    "amount_minor_units": 30,
    "currency": "EUR"
  },
  "matched_conditions": [],
  "unmet_better_program_conditions": [],
  "is_fallback": false,
  "explanation_code": "MATCHED_EXACT_PROGRAM"
}
```

## 6.6 Migration approach

1. Inventory and document current rules.
2. Build normalized transaction context.
3. Import a narrow region/network subset.
4. Run historical replay.
5. Deploy shadow evaluation.
6. Compare predictions with actual clearing.
7. Fix mappings and configuration gaps.
8. Introduce operational dashboards.
9. Activate selected downstream use cases.
10. Expand by market and product.

## 6.7 Unknown current-system details

The pack intentionally uses adapters and extension attributes because the current system stack, processor, message format, persistence technology, and deployment topology have not been supplied. Map these interfaces first; avoid embedding network-specific assumptions in the domain engine.
