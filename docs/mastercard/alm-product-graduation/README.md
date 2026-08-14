# Mastercard ALM / Product Graduation Plus — Full Package v3

Purpose: implement and certify an API-based Mastercard Account Catalog Services (ACS) / Account Level Management (ALM) capability to move an individual card account between approved product/program identifiers while keeping the same PAN and same BIN/account range, where Mastercard and issuer configuration permits it.

## Package contents

- `docs/mastercard-alm-product-migration-dotnet8-implementation-certification-v2.md` — full architecture, .NET 8 design, onboarding, verification, and certification plan.
- `docs/mastercard-alm-account-level-product-migration-dotnet8.md` — original implementation reference.
- `docs/change-implementation-guide.md` — exact areas to change in an existing system and migration sequence.
- `docs/watchouts-risks-rollback.md` — risks, failure modes, reconciliation, rollback, security and operational watch-outs.
- `docs/mastercard-verification-certification-runbook.md` — environment, evidence, Mastercard/CIS engagement and production gates.
- `docs/official-mastercard-references.md` — authoritative Mastercard documentation to validate against.
- `docs/mastercard-alm-dev-and-testing-estimate.md` — **recommended** incremental development + developer-testing estimate (**6 MD** baseline, **5–8 MD** range).
- `docs/development-testing-certification-estimate.md` — full end-to-end project estimate (~55 MD); retained for greenfield / certification-inclusive planning.
- `checklists/mastercard-alm-master-checklist.md` — stage-by-stage checklist.
- `samples/dotnet8-integration-skeleton.md` — recommended .NET 8 solution structure and integration skeleton.

## Hard implementation rules

1. Do not guess Mastercard API paths, HTTP verbs, request fields, response fields, product codes, or account-range values.
2. Freeze the final integration only against the ACS OpenAPI/API specification provisioned to the actual Mastercard Developers project.
3. Confirm Account Catalog Services + ALM + Product Graduation Plus entitlement with Mastercard/issuer operations before production work is considered complete.
4. Confirm each source → target product pair and the relevant BIN/account range.
5. Treat ambiguous write outcomes as reconciliation events. Do not blindly retry.
6. Do not update the local product state until Mastercard state is confirmed, unless the authoritative ACS flow is asynchronous and your state machine models that explicitly.
7. Keep PAN out of logs, traces, audit tables and user-facing error messages.
8. Use Mastercard-supported authentication as provisioned for the API/project.
9. Run Mastercard Sandbox/MTF validation and retain masked evidence.
10. Production enablement must be feature-flagged and progressively rolled out.
