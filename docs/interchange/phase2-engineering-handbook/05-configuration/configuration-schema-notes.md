# Schema and Authoring Notes

## Canonical representation

JSON is canonical. YAML and CSV are authoring/import formats normalized into canonical JSON.

## Decimal representation

Recommended canonical rate:

```json
{
  "percentage_bps": 30,
  "fixed_minor_units": 5,
  "fixed_currency": "EUR",
  "minimum_minor_units": null,
  "maximum_minor_units": null,
  "rounding": "HALF_UP_MINOR_UNIT"
}
```

For rates requiring finer precision than one basis point, use decimal strings:

```json
{"percentage": "0.003000"}
```

## Extension strategy

Network-specific criteria belong under a controlled namespace:

```json
{
  "extensions": {
    "visa": {"authorized_field": "value"},
    "mastercard": {}
  }
}
```

Register extension fields before production use. Unknown extensions fail validation.
