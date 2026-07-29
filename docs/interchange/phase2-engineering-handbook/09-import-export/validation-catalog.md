# Validation Catalog

## Structural
- schema version supported;
- required files present;
- checksum valid;
- unique IDs;
- valid JSON/YAML/CSV.

## Referential
- every rule references a program;
- dictionaries and currencies exist;
- source references exist;
- override target exists.

## Semantic
- valid effective periods;
- no impossible conditions;
- no unsupported network/region combination;
- valid amount/rate precision;
- fixed currency consistent with policy.

## Rule analysis
- same-precedence overlap;
- shadowed/unreachable rule;
- missing mandatory fallback;
- candidate partition too large;
- excessive condition depth;
- contradictory conditions.

## Governance
- source document metadata present;
- author differs from approver;
- production signature valid;
- environment promotion path valid.

## Regression
- bundled golden tests pass;
- portfolio simulation within approved tolerance;
- no unexpected unmatched growth;
- latency benchmark budget met.
