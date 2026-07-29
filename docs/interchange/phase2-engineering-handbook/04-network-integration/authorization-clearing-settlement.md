# Authorization, Clearing and Settlement Integration

## Authorization

At authorization time, the platform often has enough information to produce an estimate, but not necessarily the final network-assessed result.

Capture:

- card product classification;
- merchant country/MCC;
- channel and entry mode;
- authentication;
- token/stored credential indicators;
- amount and currency;
- network routing/program indicators;
- authorization timestamp and result.

## Clearing

Clearing can add or change:

- final amount/currency;
- presentment date;
- enhanced data;
- transaction subtype;
- partial shipment;
- merchant descriptor/data;
- network program/rate outcome;
- actual interchange amount.

Run the engine again using clearing context and preserve both decisions.

## Settlement

Settlement/reporting supplies authoritative financial postings. Map actual:

- network program code;
- interchange amount;
- assessment amounts separately;
- settlement currency;
- file/batch identifiers;
- adjustment reason.

## Lifecycle correlation

Use stable identifiers and fallback matching:

1. internal transaction ID;
2. network reference;
3. authorization code plus account token/hash;
4. amount/date/merchant probabilistic fallback for legacy records.

Track one-to-many relationships for partial captures and multiple clearing records.
