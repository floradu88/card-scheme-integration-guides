# Implementation notes

## Why BIN Lookup does not perform the upgrade

BIN Lookup identifies account-range / issuer / product information.

ALM Product Graduation is a PAN-level capability. This allows individual accounts
to be treated differently without requiring a change to the whole BIN/account range.

Therefore BIN Lookup is useful for:

- determining native product data;
- retrieving ICA;
- validation;
- diagnostics;
- audit enrichment.

It is not itself the Product Graduation write operation.

## Security

This implementation handles PAN data and must be designed within the applicable
PCI DSS scope.

Recommended controls:

- never log full PAN;
- mask PAN in logs/responses;
- secrets in AWS Secrets Manager / Azure Key Vault / equivalent;
- private key outside source control;
- TLS only;
- least privilege;
- request/response redaction;
- audit trail;
- idempotency/correlation IDs;
- restricted production access;
- retention/deletion policy;
- encrypted storage where persistence is unavoidable.

## Reliability

Recommended patterns:

- retry only transient failures;
- never blindly retry an ALM write without idempotency/correlation semantics;
- circuit breaker around Mastercard external calls;
- durable state machine for asynchronous final status;
- reconciliation job for submissions stuck in `SUBMITTED`;
- business-level dead-letter/review queue for rejects.

## Observability

Capture:

- correlation ID;
- masked PAN;
- source product;
- target product;
- ICA;
- submit time;
- Mastercard reference;
- final status;
- rejection code;
- latency;
- retry count.

Never capture raw secret material or unmasked PAN in normal application logs.

## Testing

1. unit tests for mapping;
2. unit tests for masking and validation;
3. OAuth signature golden tests;
4. sandbox BIN Lookup tests;
5. sandbox ALM submission tests;
6. rejection-path tests;
7. idempotency tests;
8. asynchronous status tests;
9. authorization validation;
10. clearing/reconciliation validation.
