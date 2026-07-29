# 5. Performance and Runtime

## 5.1 Fast-path design

Do not scan every rule for every transaction.

Partition candidates using a composite key:

```text
network
+ region_relation
+ transaction_type
+ product_family
+ channel
+ cardholder_type
```

Within each partition, evaluate a small ordered rule list.

## 5.2 Precompilation

At activation time:

- validate all rules;
- resolve dictionary references;
- compile conditions into predicates;
- calculate specificity;
- build immutable indexes;
- warm caches;
- run smoke tests;
- atomically swap the active in-memory snapshot.

The transaction path performs no parsing, database joins or schema validation.

## 5.3 Caching

Recommended layers:

1. Immutable in-process active snapshot.
2. Distributed cache for package retrieval and multi-node synchronization.
3. Database as durable source of truth.

Use version IDs in cache keys. Never invalidate individual rules; activate a new snapshot.

## 5.4 BIN/product data

BIN/product metadata can be high cardinality and change separately. Keep it in a dedicated versioned lookup service or compact in-memory prefix structure.

Use:
- longest-prefix match;
- effective dates;
- product/funding/commercial indicators;
- issuer country;
- source/version metadata.

## 5.5 Complexity target

With proper partitioning:

```text
candidate lookup: O(1)
rule evaluation: O(k), where k is a small partition-specific list
```

Avoid a general-purpose scripting engine in the hot path.

## 5.6 Availability

- Continue using last-known-good configuration if the configuration service is unavailable.
- Fail closed for invalid imports.
- Define business policy for unmatched live transactions: fallback, hold, alert, or no estimate.
- Persist version ID with every result.
- Support deterministic replay.

## 5.7 Suggested operational targets

Tune to your system, but a reasonable initial target is:

- p95 evaluation below 2 ms in process;
- p99 below 5 ms;
- zero network calls in the hot path;
- atomic activation under 1 second after preloading;
- full replay reproducibility;
- horizontal scaling without configuration drift.
