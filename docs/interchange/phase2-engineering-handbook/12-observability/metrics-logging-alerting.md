# Metrics, Logging and Alerting

## Metrics

Runtime:
- evaluations total;
- latency histogram;
- matches/fallbacks/unmatched;
- candidates evaluated;
- active checksum per node;
- errors by code.

Configuration:
- imports;
- validation failures;
- overlap/gap counts;
- package activation duration;
- preload failures.

Reconciliation:
- matched actuals;
- variance amount;
- unexpected downgrade;
- missing actual;
- correlation failure;
- top reason codes.

## Logging

Structured fields:

```text
decision_id
transaction_id_hash
network
region
package_version
program_id
rule_id
fallback
reason_codes
latency_us
```

Never log PAN, CVV/CVC, PIN data, cryptograms, secrets or sensitive authentication data.

## Alerts

- node configuration drift;
- fallback/unmatched spike;
- variance spike;
- new package failed preload;
- actual feed missing;
- BIN version stale;
- latency or allocation regression;
- emergency package active beyond limit.
