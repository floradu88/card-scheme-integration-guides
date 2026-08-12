# Threat Model

## STRIDE review

### Spoofing
Risks:
- stolen service credentials;
- fraudulent client certificate;
- impersonated operator.

Controls:
- mTLS;
- managed identities;
- MFA;
- certificate validation;
- privileged access controls.

### Tampering
Risks:
- modified rule package;
- altered settlement record;
- unauthorized rate change.

Controls:
- package checksums;
- digital signatures;
- immutable versions;
- audit logs;
- four-eyes approval.

### Repudiation
Risks:
- operator denies configuration change;
- transaction decision cannot be reconstructed.

Controls:
- immutable audit;
- signed release evidence;
- decision versioning;
- trace and correlation IDs.

### Information disclosure
Risks:
- payment data in logs;
- leaked private keys;
- exported proprietary rules.

Controls:
- masking;
- tokenization;
- vault;
- least privilege;
- export controls.

### Denial of service
Risks:
- Mastercard dependency failure;
- expensive rule packages;
- malformed imports;
- connection exhaustion.

Controls:
- local engine;
- resource limits;
- bounded rule complexity;
- circuit breaking;
- load shedding;
- last-known-good snapshot.

### Elevation of privilege
Risks:
- author activates own package;
- support user gains admin access.

Controls:
- RBAC;
- separation of duties;
- JIT elevation;
- access review;
- tamper-evident audit.
