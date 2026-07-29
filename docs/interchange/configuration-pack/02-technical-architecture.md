# 2. Technical Architecture

## 2.1 Logical components

```text
                       +----------------------+
Official/Internal ---> | Import & Normalizer  |
Sources                 +----------+-----------+
                                   |
                                   v
                       +----------------------+
                       | Validation Pipeline  |
                       | schema/semantic/test |
                       +----------+-----------+
                                   |
                                   v
                       +----------------------+
                       | Version Repository   |
                       | draft/approved/live  |
                       +----------+-----------+
                                  |
                         atomic activation
                                  |
                                  v
+-------------+         +----------------------+        +------------------+
| Transaction | ------> | Qualification Engine | -----> | Decision Record  |
| Context     |         | indexed + cached     |        | explainable      |
+-------------+         +----------------------+        +------------------+
                                  |
                                  v
                       +----------------------+
                       | Reconciliation       |
                       | actual vs expected   |
                       +----------------------+
```

## 2.2 Transaction context

Normalize authorization, capture and clearing data into one network-neutral object. Keep raw network fields separately for audit.

Recommended fields:

- network
- transaction type
- authorization/capture/clearing timestamps
- transaction and settlement currencies
- transaction amount
- merchant country, acquirer country and issuer country
- MCC
- merchant/terminal identifiers
- card product family and funding type
- consumer/commercial indicator
- card-present/card-not-present
- POS entry mode and terminal capabilities
- EMV/contactless/fallback flags
- e-commerce and 3DS indicators
- tokenization and wallet indicators
- stored credential/MIT/CIT indicators
- Level II/III completeness
- authorization characteristics
- clearing delay
- regulated/unregulated flags where applicable
- network-specific extension attributes

## 2.3 Evaluation stages

1. Resolve active configuration version by network, region and event date.
2. Normalize transaction attributes.
3. Derive computed attributes such as domestic/intra-regional/interregional.
4. Select candidate rule partition.
5. Apply exclusion predicates.
6. Evaluate rules in deterministic order.
7. Calculate percentage, fixed and capped components.
8. Apply rounding rules.
9. Persist decision, matched rule and explanation.
10. For clearing records, reconcile expected against actual.

## 2.4 Deterministic precedence

Avoid an implicit “first rule wins” model based on import order. Use explicit precedence:

```text
priority DESC
specificity DESC
effective_from DESC
rule_id ASC
```

Specificity can be calculated from the number and strength of constrained dimensions. A rule matching one exact MCC and exact product is more specific than a wildcard rule.

Reject ambiguous same-precedence overlaps during import unless an explicit conflict-resolution policy exists.

## 2.5 Rate calculation

Support:

```text
fee = percentage_component + fixed_component
percentage_component = transaction_amount * percentage_rate
fee = apply(minimum, maximum, cap, floor, rounding)
```

Store rates as decimal values, never binary floating point. Store monetary values in minor units or fixed precision decimals.

## 2.6 Estimated versus actual

Maintain separate concepts:

- `estimated_program` and `estimated_fee`
- `actual_network_program` and `actual_fee`
- `variance`
- `variance_reason`

Do not overwrite the estimate when actual clearing data arrives. Both are needed for model accuracy and diagnosis.

## 2.7 Network adapters

Keep Visa and Mastercard parsing outside the rule engine:

```text
Visa message/report -> Visa adapter -> normalized transaction context
Mastercard message/report -> Mastercard adapter -> normalized transaction context
```

The engine should not directly depend on ISO field numbers. Adapters translate network-specific messages into stable domain attributes.
