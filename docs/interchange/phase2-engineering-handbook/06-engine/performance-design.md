# Performance Design

## Partitioning

Primary dimensions:

```text
network|region_relation|event_type|cardholder_type|funding_type|channel
```

Secondary indexes can include MCC group, product family and transaction type.

## Hot-path constraints

- immutable objects;
- array/list iteration;
- interned enums/IDs;
- integer minor units;
- pre-parsed dates;
- no JSON parsing;
- no database access;
- no remote lookup;
- no regular expressions unless precompiled and bounded.

## BIN lookup

Use a versioned longest-prefix trie or sorted prefix structure. Keep product lookup separate from rule configuration because it changes independently.

## Cache model

- durable DB/object store;
- central configuration publisher;
- local snapshot;
- optional distributed notification;
- node health reports active checksum.

## Benchmark suite

Measure:

- exact-match partition;
- fallback partition;
- worst candidate list;
- mixed traffic;
- snapshot swap under load;
- historical replay throughput;
- diagnostic explain mode.

Record allocations/op and GC behavior, not just latency.
