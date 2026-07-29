# Business Capability and Value

## Business framing

Interchange is one component in the economics between issuer, network, acquirer and merchant. The capability should not be framed merely as a fee calculator. It is a control system for:

- financial estimation;
- scheme configuration management;
- clearing reconciliation;
- downgrade analysis;
- merchant and portfolio profitability;
- operational audit;
- change-impact simulation.

## Capability map

```text
Source Management
  -> Rule Interpretation
  -> Configuration Authoring
  -> Validation and Approval
  -> Runtime Qualification
  -> Fee Calculation
  -> Decision Explanation
  -> Clearing Reconciliation
  -> Variance Management
  -> Reporting and Audit
```

## Business outputs

- Expected interchange program and amount.
- Actual network program and amount.
- Variance amount and reason.
- Missed qualification criteria.
- Rate-change impact by portfolio.
- Fallback/downgrade frequency.
- Source traceability.
- Effective-date and market coverage.

## Financial separation

Keep distinct ledgers or measures for:

- interchange;
- network/scheme fees;
- processor fees;
- acquirer margin;
- merchant pricing;
- FX;
- dispute/chargeback fees;
- taxes.

Combining them into a single “cost” field makes reconciliation and regulatory reporting fragile.

## Operating model

### Scheme Operations
Maintains source inventory, interprets publications, creates packages and owns effective dates.

### Engineering
Owns schemas, compiler, evaluator, adapters and deployment.

### Finance
Owns expected-versus-actual tolerances, accrual use and accounting outputs.

### Support
Uses explainability and replay to investigate anomalies.

### Compliance
Reviews access, redistribution and evidence retention.

## KPIs

- actual program identification rate;
- fee variance in basis points and minor units;
- fallback percentage;
- missing-data percentage;
- configuration lead time;
- number of manual corrections;
- replay reproducibility;
- p95/p99 latency;
- active-version consistency across nodes.
