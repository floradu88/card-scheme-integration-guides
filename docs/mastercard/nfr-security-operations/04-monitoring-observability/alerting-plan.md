# Alerting Plan

## Severity 1

- payment path unavailable;
- widespread Mastercard connectivity failure;
- active configuration invalid or inconsistent;
- duplicate financial processing risk;
- settlement or reconciliation outage with material exposure;
- secret or certificate compromise.

## Severity 2

- sustained authorization error increase;
- certificate expiry inside 14 days;
- fallback/unmatched spike;
- significant financial variance;
- node configuration drift;
- delayed clearing/settlement feed.

## Severity 3

- performance degradation;
- increased parser failures;
- reconciliation backlog growth;
- certificate expiry inside 60 days;
- non-production integration failure.

## Example alerts

| Alert | Trigger |
|---|---|
| Mastercard TLS failure | Any sustained production occurrence |
| Mastercard 5xx spike | Above dynamic or fixed baseline |
| Engine p99 latency | > 5 ms for agreed duration |
| Unmatched rate | Above approved threshold |
| Fallback rate | Sudden increase or > target |
| Config drift | Node checksum differs from approved checksum |
| Settlement missing | Feed later than expected SLA |
| Certificate expiry | 90/60/30/14/7/1-day notifications |
| Reconciliation variance | Above amount or basis-point tolerance |

Alerts must be actionable and linked to a runbook.
