# 1. Business Overview

## 1.1 Purpose

The goal is to determine, explain, and report the interchange program and amount applicable to a card transaction, while allowing rate tables and qualification criteria to change without application redeployment.

The solution must support four business capabilities:

1. **Qualification** — identify the applicable interchange program.
2. **Estimation** — predict expected interchange before clearing.
3. **Reconciliation** — compare expected and actual network-assessed interchange.
4. **Optimization and diagnosis** — explain downgrades, missing data, and configuration gaps.

## 1.2 Fee flow

Interchange is generally a transfer fee between acquirer and issuer. It is not identical to the merchant discount rate. The merchant discount rate may include interchange, scheme fees, processor fees, acquirer margin, risk costs, and other commercial components.

Simplified settlement flow:

```text
Cardholder -> Merchant -> Acquirer -> Network -> Issuer
Issuer -> Network -> Acquirer: transaction amount net of applicable interchange
Acquirer -> Merchant: transaction amount net of merchant commercial charges
```

## 1.3 Business stakeholders

| Stakeholder | Responsibility |
|---|---|
| Product / Commercial | Defines supported markets, products, pricing and merchant propositions |
| Scheme Operations | Interprets Visa/Mastercard publications and effective dates |
| Finance | Reconciliation, accruals, settlement and profitability |
| Engineering | Rule engine, APIs, persistence and performance |
| Configuration Operations | Imports, validates and activates rate packages |
| Risk / Compliance | Ensures permitted use, auditability and regional compliance |
| Support | Investigates qualification mismatches and merchant queries |
| Data / BI | Reporting by network, program, market, MCC and channel |

## 1.4 Business questions the system must answer

For every evaluated transaction:

- Which network and regional program applied?
- Which configuration version was used?
- Which rule matched and why?
- What percentage and fixed amount were applied?
- Which transaction attributes were relevant?
- Was the result an estimate or a network-confirmed actual?
- Which conditions prevented a more favorable program?
- Did the transaction fall back to a default or downgrade program?
- Is the result reproducible later?

## 1.5 Core business scenarios

- Domestic consumer debit/credit.
- Intra-EEA and interregional transactions.
- Card-present, card-not-present and credential-on-file.
- EMV, contactless, magnetic-stripe fallback and manual entry.
- Tokenized wallet transactions.
- Commercial, purchasing and fleet products.
- Level II and Level III enhanced data.
- Refunds, reversals, partial clearing and incremental authorization.
- Late presentment and other timing-related downgrade paths.
- Merchant-specific, MCC-specific or regulated caps.
- Rate changes with future effective dates.

## 1.6 Success metrics

- Qualification prediction accuracy.
- Percentage of actual clearing records matched to a known rule.
- Reconciliation variance by amount and count.
- Rule evaluation latency (p50, p95, p99).
- Configuration deployment lead time.
- Import validation failure rate.
- Number of unresolved overlaps or gaps.
- Percentage of transactions using fallback rules.
- Reproducibility rate for historical decisions.
