# Certification Process

## Preconditions
- use case approved
- required product access approved
- sponsor/acquirer dependency resolved where applicable
- Sandbox implementation substantially complete
- commercial/VDP agreement track started or completed as required
- Going Live / Onboarding Dashboard initiated

## Sandbox
1. Create project and APIs.
2. Obtain sandbox credentials/test data.
3. Implement connectivity.
4. Implement mTLS and MLE where required.
5. Execute functional and negative tests.
6. Complete observability/security tests.
7. Capture evidence.

## Going Live / Onboarding
Complete:
- Business Entity Information
- Business Entity Contact
- Project Information
- Request to Start Next Environment
- API Registration where required
- Project Users
- Credentials and Encryption
- CSR(s)

Certification and Production require different CSR files when mTLS / OAuth signing is used. MLE may require a separate CSR.

## Certification
- receive Implementation Manager / certification plan
- receive acquirer/client certification credentials/test data where applicable
- configure `https://cert.api.mastercard.com/<URI>`
- run connectivity/Hello World if applicable
- execute product-defined certification cases
- capture request IDs/correlation IDs and sanitized evidence
- resolve defects and rerun
- provide PCI DSS attestation where required for PAN-bearing products
- obtain certification sign-off / production readiness

## Production
- create/use production-specific CSR and credentials
- configure `https://api.mastercard.com/<URI>`
- validate trust chain and authentication
- validate MLE
- perform approved smoke test
- activate monitoring/on-call
- controlled traffic ramp where feasible
- hypercare and reconciliation

## Mastercard Developers APIs specific
Sandbox testing and certification are required before production activation. Complete API-suite access requires acquirer + Mastercard pre-approval. Mastercard states an Implementation Manager provides a project plan and onboarding forms after the applicable agreement process.
