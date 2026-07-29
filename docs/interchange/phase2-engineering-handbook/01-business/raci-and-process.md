# RACI and Change Process

| Activity | Scheme Ops | Product | Engineering | Finance | Compliance | Operations |
|---|---|---|---|---|---|---|
| Interpret network release | R/A | C | C | C | C | I |
| Author configuration | R | C | C | C | I | I |
| Validate technical package | C | I | R/A | C | I | C |
| Approve rates | R | I | C | A | C | I |
| Activate production | C | I | C | I | I | R/A |
| Reconcile actuals | C | I | C | R/A | I | C |
| Roll back | C | I | C | I | I | R/A |

## Change lifecycle

```text
Discovered -> Draft -> Parsed -> Validated -> Simulated
-> Approved -> Scheduled -> Active -> Superseded -> Retired
```

Emergency correction:

```text
Incident -> Create correction package -> targeted validation
-> dual approval -> atomic activation -> replay affected period
```
