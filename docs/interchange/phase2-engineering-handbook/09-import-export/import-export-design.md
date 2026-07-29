# Import and Export Design

## Supported inputs

- JSON canonical package;
- YAML authoring package;
- CSV tabular rules/rates;
- ZIP bundle;
- controlled internal converter from network publications.

## ZIP layout

```text
manifest.json
programs.json
rules.json
dictionaries.json
derivations.json
tests.json
sources/source-index.json
validation/expected-baseline.json
signatures/package.sha256
```

## Pipeline

```text
Upload -> quarantine -> checksum/signature -> unpack
-> schema -> references -> semantics -> overlap/gap
-> effective dates -> rate safety -> bundled tests
-> portfolio simulation -> independent approval -> schedule
-> preload/compile -> atomic activation
```

## CSV strategy

CSV cannot represent arbitrary nested logic cleanly. Support it for common flat criteria and reject unsupported complexity. Normalize every CSV row to canonical JSON before validation.

## Export modes

- active package ZIP;
- canonical JSON;
- operations CSV;
- human-readable rule catalog;
- version diff;
- filtered network/region export;
- audit evidence package.

## Diff categories

- added/removed program;
- rate changed;
- condition changed;
- precedence changed;
- effective date changed;
- source changed;
- test changed;
- dictionary changed.

## Rollback

Activate a previous immutable package. Never edit production rows to “undo” a release.
