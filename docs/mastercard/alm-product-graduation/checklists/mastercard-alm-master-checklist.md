# Mastercard ALM Master Checklist

## A. Discovery / entitlement

- [ ] Identify issuer/processor relationship and responsible Mastercard contact.
- [ ] Confirm Account Catalog Services entitlement.
- [ ] Confirm Account Level Management capability required for the use case.
- [ ] Confirm Product Graduation Plus entitlement.
- [ ] Identify all target BIN/account ranges.
- [ ] Identify source Mastercard product code(s).
- [ ] Identify target Mastercard product code(s).
- [ ] Confirm each source -> target transition.
- [ ] Confirm downgrade/reversal direction where required.
- [ ] Confirm same PAN is retained.
- [ ] Confirm same BIN/account range is retained.
- [ ] Confirm whether Account Range Enablement is required.
- [ ] Confirm Sandbox/MTF access process.
- [ ] Confirm whether formal Mastercard certification/sign-off is required.
- [ ] Record Mastercard support/escalation route.

## B. Mastercard project / contract

- [ ] Add/enable ACS in the Mastercard Developers project.
- [ ] Create required Sandbox/MTF credentials.
- [ ] Store keys in approved secret management.
- [ ] Obtain current ACS API/OpenAPI contract.
- [ ] Record API version/release.
- [ ] Hash/version the OpenAPI file.
- [ ] Confirm exact Product Graduation operationId.
- [ ] Confirm exact HTTP method.
- [ ] Confirm exact path.
- [ ] Confirm exact request schema.
- [ ] Confirm exact response schema.
- [ ] Confirm error schema.
- [ ] Confirm authentication mechanism for this API/project.
- [ ] Confirm production endpoint and credential process.

## C. Application design

- [ ] Add network-neutral product migration interface.
- [ ] Add Mastercard ALM adapter.
- [ ] Keep Mastercard DTOs inside infrastructure layer.
- [ ] Add product mapping repository/configuration.
- [ ] Add allowed BIN/account-range configuration.
- [ ] Add allowed source/target product-pair configuration.
- [ ] Add migration state machine.
- [ ] Add idempotency model.
- [ ] Add migration audit persistence.
- [ ] Add verification client.
- [ ] Add reconciliation worker.
- [ ] Add feature flags.
- [ ] Add kill switch.
- [ ] Define local source of truth vs Mastercard effective-state rules.

## D. Existing-system impact review

- [ ] Search for assumptions that product change requires card reissue.
- [ ] Search for assumptions that BIN uniquely identifies product.
- [ ] Search for immutable card-product fields.
- [ ] Review authorization/risk rules.
- [ ] Review pricing/fee logic.
- [ ] Review interchange/product classification.
- [ ] Review loyalty/benefit eligibility.
- [ ] Review statements.
- [ ] Review customer-service tooling.
- [ ] Review customer notifications.
- [ ] Review CRM.
- [ ] Review data warehouse/reporting.
- [ ] Review fraud/risk models.
- [ ] Review wallet/token metadata dependencies.
- [ ] Review disputes/chargebacks.
- [ ] Review caches.
- [ ] Define downstream product-changed event/replay.

## E. Security / PCI

- [ ] Document PCI data flow.
- [ ] Keep clear PAN inside PCI-controlled boundary.
- [ ] Do not persist clear PAN in migration table.
- [ ] Redact PAN from logs.
- [ ] Redact PAN from traces.
- [ ] Do not use PAN in metric labels.
- [ ] Redact Authorization headers.
- [ ] Protect signing keys in approved secret/HSM mechanism.
- [ ] Implement key rotation procedure.
- [ ] Test key rotation.
- [ ] Restrict service permissions.
- [ ] Restrict network egress.
- [ ] Complete threat model.
- [ ] Add admin endpoint authorization.
- [ ] Add rate limits.
- [ ] Add audit actor/reason.
- [ ] Assess four-eyes approval for sensitive/bulk operations.
- [ ] Add detection for abnormal migration volumes.

## F. Implementation

- [ ] Generate/bind .NET client from exact Mastercard contract.
- [ ] Implement authentication/signing.
- [ ] Implement product eligibility validation.
- [ ] Implement account-range guard.
- [ ] Implement product pair guard.
- [ ] Implement Product Graduation submission.
- [ ] Capture Mastercard request/correlation ID.
- [ ] Implement resulting-state verification.
- [ ] Update local programme only after confirmed network outcome.
- [ ] Implement NoChange behavior.
- [ ] Implement idempotency conflict handling.
- [ ] Implement error mapping.
- [ ] Implement Unknown state.
- [ ] Implement reconciliation.
- [ ] Implement ManualReview path.
- [ ] Implement structured redacted logging.
- [ ] Implement metrics.
- [ ] Implement alerting.

## G. Unit / contract tests

- [ ] Same PAN invariant test.
- [ ] Same BIN invariant test.
- [ ] Allowed product pair test.
- [ ] Unsupported product pair test.
- [ ] Disabled BIN/account-range test.
- [ ] Already target product test.
- [ ] Duplicate idempotency key test.
- [ ] Same key/different payload conflict test.
- [ ] Mastercard rejection does not alter local state.
- [ ] PAN absent from logs.
- [ ] Auth header absent from logs.
- [ ] Exact generated contract serialization test.
- [ ] Error mapping tests.
- [ ] State-machine transition tests.
- [ ] Reconciliation tests.

## H. Mastercard Sandbox / MTF

- [ ] Obtain Mastercard-approved test data.
- [ ] Validate authentication.
- [ ] Validate account-range setup.
- [ ] Run Product A -> Product B.
- [ ] Verify Mastercard resulting product state.
- [ ] Verify PAN unchanged.
- [ ] Verify BIN/account range unchanged.
- [ ] Run Product B -> Product A if approved.
- [ ] Test invalid target product.
- [ ] Test disabled account range.
- [ ] Test invalid/unknown PAN.
- [ ] Test duplicate request/business operation.
- [ ] Test auth/signature failure.
- [ ] Test rate limiting where feasible.
- [ ] Test 5xx behavior.
- [ ] Test timeout/ambiguous outcome.
- [ ] Demonstrate reconciliation.
- [ ] Test local DB failure after Mastercard success.
- [ ] Test downstream event replay.
- [ ] Save masked certification evidence.

## I. Operational readiness

- [ ] Dashboard deployed.
- [ ] Alerts deployed.
- [ ] Unknown outcome alert defined.
- [ ] State mismatch alert defined with zero tolerance.
- [ ] Auth failure alert defined.
- [ ] Reconciliation backlog alert defined.
- [ ] Runbook reviewed.
- [ ] Kill switch tested.
- [ ] Feature flag tested.
- [ ] Manual repair procedure tested.
- [ ] On-call support trained.
- [ ] Mastercard escalation details available.

## J. Certification / approval

- [ ] Prepare evidence bundle.
- [ ] Include API/OpenAPI version/hash.
- [ ] Include test cases/results.
- [ ] Include masked request/response evidence.
- [ ] Include correlation IDs.
- [ ] Include same-PAN/same-BIN evidence.
- [ ] Include reconciliation evidence.
- [ ] Include security sign-off.
- [ ] Include operational sign-off.
- [ ] Submit to Mastercard/issuer team if required.
- [ ] Resolve certification defects.
- [ ] Record Mastercard/issuer acceptance.
- [ ] Obtain production credentials/entitlement.

## K. Production rollout

- [ ] Deploy with feature disabled.
- [ ] Validate production configuration.
- [ ] Validate secrets/key loading.
- [ ] Validate safe connectivity.
- [ ] Enable pilot card/BIN/product pair only.
- [ ] Execute approved pilot.
- [ ] Verify Mastercard state.
- [ ] Verify local state.
- [ ] Verify downstream state.
- [ ] Verify authorization/benefits/pricing where applicable.
- [ ] Monitor Unknown/reconciliation metrics.
- [ ] Expand gradually.
- [ ] Stop rollout immediately on state mismatch.
- [ ] Retain production validation evidence.

## L. Post-launch

- [ ] Review first production migrations.
- [ ] Review reconciliation queue daily during rollout.
- [ ] Review rejects by code.
- [ ] Review product-state mismatch metric.
- [ ] Review customer-service issues.
- [ ] Review downstream lag/cache problems.
- [ ] Confirm no unintended reissues.
- [ ] Confirm no duplicate card/token creation.
- [ ] Review key/certificate expiry monitoring.
- [ ] Schedule periodic Mastercard contract/version review.
