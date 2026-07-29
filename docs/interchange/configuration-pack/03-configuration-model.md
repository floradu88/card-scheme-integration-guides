# 3. Configuration Model

## 3.1 Configuration hierarchy

```text
ConfigurationPackage
  -> Metadata
  -> Dictionaries
  -> Networks
  -> Regions
  -> Programs
  -> Rules
  -> Rates
  -> Rounding policies
  -> Validation policies
  -> Test cases
```

## 3.2 Package metadata

Required:

- package ID
- semantic version
- source
- source publication date
- import timestamp
- effective period
- target environment
- network
- market/region
- checksum
- author and approver
- status: draft, validated, approved, active, retired, rejected

## 3.3 Program

A program is a named network interchange category.

Recommended fields:

- internal program ID
- network program code, when authorized and available
- public/display name
- network
- region and market
- transaction type
- product family
- channel
- description
- effective dates
- source reference
- priority
- fallback indicator

## 3.4 Rule conditions

Use a small declarative condition language rather than arbitrary code.

Supported operators:

- equals / not equals
- in / not in
- range
- exists / missing
- prefix or category membership
- date range
- numeric comparison
- all / any / none groups

Example:

```json
{
  "all": [
    {"field": "network", "operator": "equals", "value": "MASTERCARD"},
    {"field": "region_relation", "operator": "equals", "value": "INTRA_EEA"},
    {"field": "funding_type", "operator": "equals", "value": "CREDIT"},
    {"field": "cardholder_type", "operator": "equals", "value": "CONSUMER"},
    {"field": "channel", "operator": "equals", "value": "CARD_PRESENT"}
  ]
}
```

## 3.5 Derived attributes

Centralize derivations so every rule uses the same definitions:

- `region_relation`
- `is_domestic`
- `is_intra_eea`
- `is_interregional`
- `clearing_delay_hours`
- `enhanced_data_level`
- `is_authenticated_ecommerce`
- `is_tokenized`
- `is_fallback`
- `is_regulated_consumer_product`

Version derivation logic independently when definitions change.

## 3.6 Rate object

```json
{
  "percentage_bps": 30,
  "fixed_minor_units": 0,
  "currency": null,
  "minimum_minor_units": null,
  "maximum_minor_units": null,
  "rounding_policy": "HALF_UP_MINOR_UNIT"
}
```

Use basis points or a decimal string. Avoid percentages stored as binary floats.

## 3.7 Overrides

Supported override scopes:

1. Global default
2. Network
3. Region
4. Country pair
5. Acquirer
6. Merchant group
7. Merchant
8. Terminal

Overrides must not silently mutate base rules. Store them as separate, effective-dated layers with explicit precedence and audit history.

## 3.8 Configuration environments

- Development
- Test
- Certification/UAT
- Shadow production
- Production

A package moves through environments by promotion, not by manual recreation.
