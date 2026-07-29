# Configuration Specification

## Package structure

```text
manifest
dictionaries
programs
rules
rates
overrides
derivations
tests
sources
signatures
```

## Scopes

- network;
- region;
- country/corridor;
- acquirer;
- merchant group;
- merchant;
- terminal.

## Effective dating

Every program, rule, rate, dictionary entry and override supports:

- `effective_from`;
- `effective_to`;
- correction/replacement relationship;
- source publication and announced effective date.

Select based on transaction event date, not current server date.

## Conditions

Supported operators:

- `equals`, `not_equals`;
- `in`, `not_in`;
- `exists`, `missing`;
- `gt`, `gte`, `lt`, `lte`;
- `range`;
- `prefix`;
- `member_of_dictionary`;
- nested `all`, `any`, `none`.

Do not permit user-provided code.

## Priority and specificity

Priority is explicit business precedence. Specificity is compiler-calculated from constrained dimensions.

## Fallbacks

Every mandatory partition should have an intentional outcome:

- exact program;
- fallback/downgrade;
- unmatched with alert;
- not applicable.

Never invent a rate merely to prevent an unmatched result.
