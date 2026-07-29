# Current-System Field-Mapping Template

Complete one row per normalized field.

| Normalized field | Current source | Visa source | Mastercard source | Auth | Clearing | Required | Transformation | Owner |
|---|---|---|---|---|---|---|---|---|
| network | | | | Y | Y | Y | enum mapping | |
| merchant.mcc | | | | Y | Y | Y | left-pad 4 | |
| merchant.country | | | | Y | Y | Y | ISO alpha-2 | |
| issuer.country | | | | Y | Y | conditional | BIN lookup | |
| card.funding_type | | | | Y | Y | Y | product lookup | |
| acceptance.entry_mode | | | | Y | Y | Y | mapping table | |
| authentication.three_ds | | | | Y | Y | CNP | derive | |
| timing.clearing_delay_hours | | | | N | Y | timing rules | calculate | |

## Mapping rules

- Every transformation must be versioned.
- Unknown values stay `UNKNOWN`; do not silently default.
- Record confidence/source for inferred data.
- Never map proprietary codes by guesswork.
