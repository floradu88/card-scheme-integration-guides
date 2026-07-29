# Qualification Engine

## Compilation

At package activation:

1. schema validation;
2. referential validation;
3. semantic validation;
4. resolve dictionaries;
5. normalize conditions;
6. calculate partition key and specificity;
7. compile predicates;
8. sort candidate lists;
9. execute bundled tests;
10. build immutable snapshot;
11. atomically publish.

## Runtime algorithm

```text
snapshot = activeSnapshot.for(eventDate, network, region)
context = derive(normalizedTransaction)
partition = snapshot.lookup(partitionKey(context))
for rule in partition.rules:
    if rule.matches(context):
        return calculate(rule, context)
return partition.unmatchedPolicy
```

## Explainability

Compile two predicate forms:

- fast boolean predicate;
- diagnostic predicate returning pass/fail reasons.

Use fast mode normally. Re-run diagnostic mode for sampled, failed, disputed or support-requested transactions.

## Better-program analysis

Optional diagnostic capability:

- evaluate preferred/lower-cost programs;
- list unmet requirements;
- do not claim merchant optimization unless business precedence and network eligibility are fully modeled.

## Rule safety

- maximum nesting depth;
- maximum conditions per rule;
- operator allowlist;
- field allowlist;
- candidate count budget;
- import timeout;
- no reflection-based arbitrary property access.
