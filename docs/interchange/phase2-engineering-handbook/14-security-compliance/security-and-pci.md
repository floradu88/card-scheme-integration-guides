# Security and PCI Considerations

## Configuration data

Do not include:
- PAN;
- CVV/CVC;
- PIN/PIN block;
- full track data;
- cryptographic keys;
- private certificates;
- production secrets.

## Replay data

Use:
- tokenized account references;
- hashes with controlled keys;
- synthetic or masked samples;
- segregated access;
- retention policy.

## Package security

- SHA-256 checksum;
- digital signature for production;
- malware scanning;
- decompression limits;
- path traversal prevention;
- schema and size limits;
- RBAC;
- audit trail.

## Documentation access

Portal-only Visa/Mastercard documents must follow contractual access and redistribution controls. Store references and checksums; do not copy them into broad-access repositories unless authorized.
