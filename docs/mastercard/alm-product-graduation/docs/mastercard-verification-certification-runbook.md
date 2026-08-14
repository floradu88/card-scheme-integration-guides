# Mastercard Verification and Certification Runbook

## Authoritative-source rule

The final production contract must be based on the Mastercard ACS API specification and credentials provisioned to the real project. Public documentation is used to establish the capability and onboarding approach; project-specific documentation/configuration is authoritative for the exact contract.

## Mastercard engagement

Ask the Mastercard representative / issuer operations / relevant CIS or implementation team to confirm:

- ACS entitlement;
- ALM entitlement;
- Product Graduation Plus entitlement;
- target account ranges;
- approved product codes;
- supported source/target product transitions;
- Account Range Enablement requirements;
- Sandbox/MTF access;
- formal test requirements;
- exact API release/version;
- production provisioning;
- incident/support process.

## Evidence bundle

For every certification run, retain:

```text
scope
environment identifiers
API version/OpenAPI hash
masked test-card references
source/target product pair
account-range reference
test case id
request timestamp
Mastercard correlation/request id
HTTP result
verified resulting product state
same-PAN assertion
same-BIN assertion
application result
logs/metrics screenshots or exports
defects
sign-off
```

Never include clear PAN, signing private key, key password, or Authorization header.

## Technical gates

### Gate 1 — Entitlement
PASS when Mastercard/issuer confirms ACS + required ALM/PGP services and target account range/product pair.

### Gate 2 — Authentication
PASS when a signed request reaches Mastercard Sandbox/MTF and expected gateway behavior is observed.

### Gate 3 — Contract
PASS when the exact API/OpenAPI release is pinned and generated/bound client tests succeed.

### Gate 4 — Positive migration
PASS when an approved PAN moves source product -> target product and verification confirms target state.

### Gate 5 — Invariants
PASS when PAN and BIN/account range are demonstrably unchanged.

### Gate 6 — Negative/error suite
PASS when invalid product, invalid PAN, disabled range, auth errors, 429/5xx and duplicate attempts do not corrupt state.

### Gate 7 — Ambiguous-outcome reconciliation
PASS when a simulated/real timeout after submission does not cause a blind duplicate write and the system reaches the correct final state via verification.

### Gate 8 — PCI/security
PASS when redaction, secret storage, access control, threat model and security review are complete.

### Gate 9 — Operations
PASS when dashboards, alerts, runbooks, kill switch, manual repair and support escalation work.

### Gate 10 — Mastercard/issuer sign-off
PASS only when the required Mastercard/issuer approval for production is recorded.

## Production smoke test

After deployment but before broad activation:

```text
1. Feature flag OFF.
2. Verify production authentication/connectivity using an approved safe operation/path.
3. Verify monitoring and redaction.
4. Enable only approved pilot card(s)/range/product pair.
5. Perform one approved migration.
6. Verify Mastercard state.
7. Verify issuer local state.
8. Verify downstream product representation.
9. Check authorization/benefits/pricing behavior if in scope.
10. Keep evidence.
11. Expand only after sign-off.
```
