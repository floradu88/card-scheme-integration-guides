# Capacity Planning

## Inputs

- current TPS;
- peak TPS;
- authorization/capture/clearing mix;
- growth rate;
- replay volume;
- number of networks;
- configuration size;
- average candidate rules;
- storage growth;
- reconciliation window.

## Headroom

Recommended planning:

- normal peak below 60–70% sustained resource capacity;
- capacity for one node/zone failure;
- additional headroom during deployments;
- separate replay capacity from live payment capacity.

## Test profiles

- steady state;
- daily peak;
- burst;
- node failure;
- Mastercard latency;
- configuration activation under load;
- backlog recovery;
- end-of-day clearing spike;
- month-end reporting;
- regional failover.

## Outputs

- required instance count;
- autoscaling thresholds;
- database sizing;
- queue partitions;
- storage and retention;
- network bandwidth;
- cost forecast.
