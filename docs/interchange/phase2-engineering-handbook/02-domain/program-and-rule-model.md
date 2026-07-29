# Program and Rule Model

## Program

A program is the business result selected by qualification.

Fields:

- internal ID;
- network code when legally/contractually available;
- display name;
- network;
- region;
- product and transaction scope;
- effective period;
- source reference;
- fallback/downgrade indicator.

## Rule

A rule binds conditions to a program and rate.

```text
Rule
  ID
  Program
  Partition Key
  Priority
  Specificity
  Conditions
  Exclusions
  Rate
  Effective Period
  Source
  Tags
```

## Determinism

Selection order:

1. active package by event date;
2. partition;
3. priority descending;
4. specificity descending;
5. effective-from descending;
6. stable rule ID.

Ambiguous same-rank overlaps are activation errors.

## Rate components

- percentage;
- fixed fee;
- minimum;
- maximum/cap;
- currency-specific fixed component;
- amount bands;
- rounding policy;
- optional regulated cap;
- optional alternate refund handling.

Use decimal arithmetic or integer basis points and minor units.
