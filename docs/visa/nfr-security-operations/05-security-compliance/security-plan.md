# Security Plan

## Identity and access management

- OIDC/OAuth2 for human and service access.
- MFA for privileged users.
- least privilege.
- separation of duties.
- just-in-time privileged elevation where possible.
- service identities instead of shared accounts.
- periodic access review.

## Recommended roles

- Viewer;
- Support;
- Operations;
- Configuration Author;
- Configuration Approver;
- Release Manager;
- Security Administrator;
- Auditor.

No single user should author, approve, and activate a production package.

## Secrets management

Use:

- Azure Key Vault;
- AWS Secrets Manager;
- HashiCorp Vault;
- HSM-backed key storage where required.

Never store secrets in:

- source code;
- Git;
- plain JSON;
- Docker images;
- tickets;
- logs;
- documentation packages.

## Certificate lifecycle

- inventory;
- issuance;
- secure storage;
- deployment;
- monitoring;
- rotation;
- revocation;
- incident handling;
- evidence retention.

Alert at:

- 90 days;
- 60 days;
- 30 days;
- 14 days;
- 7 days;
- 1 day.

Support overlapping old/new certificates for zero-downtime rotation where Visa permits.

## Encryption

- TLS 1.2 or higher according to current Visa requirements.
- mutual TLS for Visa connectivity.
- encryption at rest.
- encrypted backups.
- managed key rotation.
- restricted key access.
- no weak algorithms.

## Data protection

Classify:

- cardholder data;
- sensitive authentication data;
- personal data;
- merchant data;
- financial data;
- scheme-confidential material;
- operational metadata.

Apply:

- masking;
- tokenization;
- minimization;
- retention;
- secure deletion;
- access logging;
- geographic constraints.

## Secure development

- threat modeling;
- code review;
- SAST;
- DAST;
- dependency scanning;
- secret scanning;
- container scanning;
- IaC scanning;
- penetration testing;
- secure coding standards;
- patch SLAs.

## Supply-chain security

- signed build artifacts;
- SBOM;
- pinned dependencies;
- trusted package sources;
- reproducible builds where practical;
- protected CI/CD;
- artifact provenance;
- release approvals.

## API security

- mTLS;
- allowlisted operations;
- schema validation;
- payload size limits;
- rate limits;
- timeout limits;
- replay protection;
- operation-specific idempotency;
- safe error messages.
