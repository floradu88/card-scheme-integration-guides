# 4. Import, Export and Versioning

## 4.1 Supported formats

Recommended:

- JSON — canonical machine format.
- YAML — human-maintained configuration.
- CSV — bulk rate/rule editing for operations.
- ZIP package — configuration plus metadata, schemas, sources and tests.

JSON is the source of truth after normalization.

## 4.2 ZIP package layout

```text
package.zip
  manifest.json
  programs.json
  rules.json
  rates.json
  dictionaries.json
  tests.json
  sources/
    source-index.json
  signatures/
    checksum.sha256
```

## 4.3 Import pipeline

```text
Upload
 -> malware/file checks
 -> unpack
 -> checksum/signature verification
 -> JSON schema validation
 -> dictionary/reference validation
 -> semantic validation
 -> overlap and gap analysis
 -> rate and currency validation
 -> effective-date validation
 -> regression tests
 -> simulation
 -> approval
 -> atomic activation
```

## 4.4 Validation rules

Reject packages with:

- unknown network, country, currency, product or MCC references;
- invalid or reversed effective dates;
- duplicate IDs;
- ambiguous equal-priority overlaps;
- missing fallback for a mandatory partition;
- unreachable rules;
- unsupported operators;
- invalid decimal precision;
- percentage outside configured safe limits;
- missing source references;
- broken test expectations;
- backward effective dates without an explicit correction workflow.

## 4.5 Versioning

Use immutable semantic versions:

- major: breaking schema or interpretation change;
- minor: new programs/rules;
- patch: non-breaking correction.

Each activated version gets:

- content checksum;
- activation timestamp;
- activated-by identity;
- source package;
- approval record;
- previous version pointer.

Never update active rows in place.

## 4.6 Export

Exports should support:

- full active package;
- package by effective date;
- diff between versions;
- rules filtered by network/region;
- human-readable CSV;
- canonical JSON;
- audit bundle with source references and test results.

## 4.7 Rollback

Rollback is activation of the prior immutable version. Do not attempt to reverse individual database mutations.

## 4.8 Security and governance

- Role-based import/export permissions.
- Four-eyes approval.
- Signed packages for production.
- Encryption in transit and at rest.
- Complete audit event stream.
- Redact proprietary network content from unauthorized exports.
