# Reference Data Model

## Tables/collections

- configuration_package;
- source_document;
- dictionary;
- dictionary_entry;
- interchange_program;
- interchange_rule;
- rate_definition;
- override_rule;
- bundled_test;
- validation_run;
- approval;
- activation;
- decision;
- actual_interchange;
- reconciliation_result;
- audit_event.

## Immutability

Activated package content is immutable. Approval and activation are separate records, not mutable columns where stronger audit is required.

## Transaction decision retention

Retain:

- decision ID;
- transaction ID;
- event timestamp;
- engine/config/derivation/BIN versions;
- program and rule;
- rate components;
- input hash;
- compact reasons;
- actual outcome and variance;
- raw evidence references.

## Indexing

- active package by network/region/effective date;
- rule partition key;
- transaction ID;
- network reference;
- reconciliation status;
- variance magnitude;
- configuration version.
