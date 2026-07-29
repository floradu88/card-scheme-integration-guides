# Visa Onboarding Steps — Option E — Clearing Reconciliation Only

## 1. Confirm organizational role

- Licensed acquirer
- Acquirer processor
- Sponsored technology provider
- Gateway
- Merchant working through an acquirer

## 2. Confirm product fit

Validate with the acquirer and Visa that VisaNet Connect – Acceptance is the correct integration product for this option.

## 3. Create Visa Developer access

- Create an organization-owned Visa Developer account.
- Create a project.
- Select the required VisaNet Connect Acceptance APIs.
- Keep sandbox, certification, and production configurations separate.

## 4. Sandbox

- Obtain sandbox credentials.
- Configure mutual TLS.
- Validate DNS, firewall, trust chain, client certificate, and HTTP behavior.
- Execute synthetic positive and negative tests.
- Record limitations between sandbox and production.

## 5. Formal onboarding

Prepare:
- legal entity and region;
- acquiring sponsor;
- processor role;
- use case;
- volume and transaction types;
- architecture and hosting;
- PCI/security status;
- operations and incident contacts.

## 6. Certification

- Obtain certification endpoint and credentials.
- Use production-like binaries and network configuration.
- Execute Visa/acquirer-provided test cases.
- retain evidence and sign-off.

## 7. Production

- Obtain production entitlement and credentials.
- configure production trust and certificates;
- deploy certificate-expiry monitoring;
- run shadow/canary where applicable;
- maintain rollback and incident procedures.
