# Test Strategy

## Unit

- each operator;
- nested groups;
- derived attributes;
- rounding;
- effective dates;
- precedence;
- partitioning.

## Configuration

- schema;
- references;
- overlaps;
- gaps;
- unreachable rules;
- package signature;
- source metadata.

## Golden tests

Every program requires:
- positive match;
- near miss;
- fallback;
- effective-date boundary;
- amount/rounding boundary.

## Historical replay

Run representative portfolio data across:
- networks;
- countries;
- products;
- MCCs;
- channels;
- token/authentication types;
- transaction lifecycle;
- high-value and edge cases.

## Performance

Use BenchmarkDotNet for in-process engine benchmarks and a load tool for APIs. Include snapshot activation under load.

## Certification

Network/processor certification scope is contract and integration dependent. Maintain a checklist with:
- connectivity;
- authentication/certificates;
- message scenarios;
- clearing;
- settlement;
- reversals/refunds;
- negative/error cases;
- evidence and sign-off.

Do not claim certification based only on public documentation.
