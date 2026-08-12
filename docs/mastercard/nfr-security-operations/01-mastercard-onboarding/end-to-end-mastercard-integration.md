# End-to-End Mastercard Integration Plan

## 1. Confirm implementation option

Possible delivery modes:

- authorization-only proof of concept;
- authorization, capture, and clearing;
- full payment-processing integration;
- interchange estimation only;
- clearing reconciliation only;
- replacement of an existing processor connection.

## 2. Confirm organizational role

Mastercard and the acquiring sponsor need to know whether the organization is:

- a licensed acquirer;
- an acquirer processor;
- a gateway;
- a sponsored technology provider;
- a merchant integrating through an acquirer;
- a processor replacing an existing connection.

## 3. Establish sponsorship

A sandbox authorization experiment may be available through Mastercard Developers, but full-suite certification and production typically require:

- participating acquirer approval;
- Mastercard approval;
- contractual and operational onboarding;
- assigned implementation contacts;
- certification scope and evidence.

## 4. Create Mastercard Developers access

- Use an organization-owned account.
- Create a project.
- Select Mastercard Developers APIs and required APIs.
- Keep sandbox, certification, and production environments separate.
- Record project IDs, API versions, contacts, and ownership.

## 5. Obtain sandbox credentials

Typical material includes:

- client certificate;
- private key;
- client credentials;
- sandbox endpoint;
- Mastercard CA certificates;
- synthetic test data;
- product-specific documentation.

Store all secrets in an approved vault, never in Git or plain-text configuration.

## 6. Implement mutual TLS

The client must:

- trust Mastercard's server certificate chain;
- present the project client certificate;
- load the private key securely;
- validate certificate expiration;
- use supported TLS settings;
- provide certificate rotation without downtime.

## 7. Prove connectivity

Validate:

- DNS;
- firewall and proxy;
- TLS handshake;
- client certificate presentation;
- API authentication;
- request correlation;
- HTTP status handling;
- safe logging;
- timeout behavior.

## 8. Implement Mastercard adapter

Recommended boundary:

```text
Internal Payment Request
    -> Mastercard Request Mapper
    -> Mastercard API Client
    -> Mastercard Response Mapper
    -> Internal Payment Result
    -> Normalized Interchange Context
```

Do not leak Mastercard-specific transport models into the core payment domain.

## 9. Map Mastercard data

Create a controlled mapping catalog for:

- amount and currency;
- MCC and merchant country;
- issuer and acquirer country;
- card product;
- POS entry mode;
- card-present/card-not-present;
- tokenization;
- 3-D Secure;
- stored credential;
- authorization timestamps;
- clearing and settlement outcomes;
- network program and fee results.

Every mapping must have an owner, source, transformation, confidence, and version.

## 10. Estimate versus actual

At authorization:

- calculate an estimate;
- store rule, program, version, amount, and reasons.

At clearing/settlement:

- recalculate using final context;
- store actual network result;
- reconcile expected versus actual;
- never overwrite the original estimate.

## 11. Full-suite onboarding

Prepare:

### Business
- legal entity;
- country and region;
- acquiring sponsor;
- use case;
- merchant segments;
- transaction types;
- volume;
- target go-live date.

### Technical
- architecture;
- hosting;
- IP/network;
- API operations;
- security;
- certificates;
- PCI status;
- HA and DR;
- operational support model.

### Integration
- authorization;
- capture;
- clearing;
- settlement;
- reversal;
- refund;
- tokenized transactions;
- stored credentials;
- 3-D Secure;
- merchant and terminal data.

## 12. Certification

Certification should use:

- separate credentials;
- separate endpoint;
- production-like binaries;
- production-like deployment;
- synthetic or approved certification data;
- formal evidence capture;
- Mastercard/acquirer sign-off.

## 13. Production

Production readiness requires:

- production entitlement;
- production certificates;
- production credentials;
- approved network path;
- monitoring and alerts;
- certificate rotation;
- runbooks;
- shadow and canary rollout;
- rollback path.

## 14. Request list for Mastercard/acquirer

Request:

1. confirmation of the correct Mastercard product;
2. confirmation of organizational role;
3. sandbox and certification entitlement;
4. certification endpoints and credentials;
5. current implementation manuals;
6. message definitions;
7. applicable regional qualification material;
8. clearing and settlement interfaces;
9. actual interchange/program fields;
10. certification test cases;
11. security and PCI prerequisites;
12. source-IP/network requirements;
13. certificate lifecycle process;
14. support and incident contacts;
15. production promotion process;
16. document-storage and redistribution rules.
