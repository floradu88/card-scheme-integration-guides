# Reconciliation Design

## Objective

Compare predicted/qualified results with authoritative network or processor settlement outcomes.

## Inputs

- authorization decision;
- clearing decision;
- settlement/network actual;
- adjustments;
- configuration and parser versions.

## Match statuses

- exact program and fee;
- program match, fee mismatch;
- fee match, program unavailable;
- expected fallback;
- unexpected downgrade;
- actual source missing;
- prediction unmatched;
- correlation failure;
- currency/rounding difference.

## Variance waterfall

Classify in this order:

1. lifecycle correlation;
2. amount/currency difference;
3. event/effective date;
4. product/BIN classification;
5. region/country mapping;
6. merchant/MCC;
7. channel/entry mode;
8. authentication/token/stored credential;
9. clearing timing;
10. enhanced data;
11. rule precedence;
12. source version;
13. rounding.

## Feedback loop

Do not automatically “learn” production rules from mismatches. Generate a proposed correction with evidence, then follow normal review and approval.
