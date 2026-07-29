# 7. Testing, Observability and Rollout

## 7.1 Test layers

- Schema tests.
- Configuration semantic tests.
- Unit tests for operators and derivations.
- Golden test cases per program.
- Boundary tests for amount, date and timing ranges.
- Overlap and gap tests.
- Historical transaction replay.
- Network/acquirer certification tests where required.
- Performance and soak tests.
- Disaster recovery and rollback tests.

## 7.2 Golden test case

Each rule should include at least:

- one positive case;
- one near-miss case;
- one fallback case;
- effective-date boundary cases.

## 7.3 Explainability

Store compact reason codes in the transaction record and verbose explanations in diagnostic storage.

Example:

```text
MATCH:
- network = MASTERCARD
- region_relation = INTRA_EEA
- product = CONSUMER_CREDIT
- channel = CARD_PRESENT

NOT SELECTED LOWER-PRIORITY/FALLBACK:
- fallback rule evaluated after exact program

POTENTIAL BETTER PROGRAM NOT MET:
- enhanced_data_level required LEVEL_III, actual NONE
```

## 7.4 Metrics

- evaluations/sec;
- p50/p95/p99 latency;
- match/fallback/unmatched counts;
- active version per node;
- configuration drift;
- expected vs actual variance;
- top downgrade reasons;
- missing attribute frequency;
- import validation failures;
- activation and rollback events.

## 7.5 Rollout

Use:

1. offline historical replay;
2. test environment;
3. production shadow mode;
4. limited market/product canary;
5. broader read-only reporting;
6. controlled financial usage.

## 7.6 Audit record

Persist:

- normalized input hash;
- raw source reference;
- decision timestamp;
- event date;
- engine version;
- configuration version;
- derivation version;
- matched program and rule;
- calculated components;
- explanation codes;
- actual result and variance when available.
