# Normalized Transaction Model

## Principle

The rule engine consumes a stable domain model, not raw ISO 8583 elements, proprietary clearing columns or network API field names.

## Core object

```json
{
  "transaction_id": "string",
  "event_type": "AUTHORIZATION|CAPTURE|CLEARING|REFUND|REVERSAL",
  "event_timestamp": "ISO-8601",
  "network": "VISA|MASTERCARD",
  "amount": {"minor_units": 10000, "currency": "EUR"},
  "merchant": {
    "id": "string",
    "country": "RO",
    "mcc": "5812",
    "group_id": "optional"
  },
  "acquirer": {"country": "RO", "institution_id": "optional"},
  "issuer": {"country": "DE", "institution_id": "optional"},
  "card": {
    "funding_type": "DEBIT|CREDIT|PREPAID|UNKNOWN",
    "cardholder_type": "CONSUMER|COMMERCIAL|UNKNOWN",
    "product_family": "string",
    "bin_version": "string"
  },
  "acceptance": {
    "channel": "CARD_PRESENT|ECOMMERCE|MOTO|OTHER",
    "entry_mode": "CHIP|CONTACTLESS|MAGSTRIPE|MANUAL|CREDENTIAL_ON_FILE",
    "terminal_capability": "string",
    "emv": true,
    "fallback": false
  },
  "authentication": {
    "three_ds": true,
    "result": "string",
    "cavv_present": true
  },
  "credential": {
    "tokenized": true,
    "wallet": "APPLE_PAY",
    "stored_credential": true,
    "initiator": "CIT|MIT"
  },
  "enhanced_data": {
    "level": "NONE|LEVEL_II|LEVEL_III",
    "complete": false
  },
  "timing": {
    "authorized_at": "ISO-8601",
    "cleared_at": "ISO-8601",
    "clearing_delay_hours": 20
  },
  "extensions": {}
}
```

## Derived attributes

Compute once:

- domestic/intra-regional/interregional;
- EEA/UK/other regulatory relationship;
- presentment timeliness;
- authenticated e-commerce category;
- fallback status;
- enhanced-data completeness;
- product regulation eligibility;
- amount band;
- MCC group;
- country corridor.

## Raw evidence

Store raw inputs outside the normalized model:

```text
raw_message_reference
raw_report_reference
parser_version
source_system
source_row_or_message_id
```

Never place PAN, CVV/CVC, PIN data or other prohibited authentication data into diagnostic configuration or replay payloads.
