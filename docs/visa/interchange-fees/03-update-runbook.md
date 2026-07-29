# 3. Country fee update runbook

Generated: 2026-07-29

## Purpose

Ingest a Visa interchange fee update for one or more countries, validate it, simulate impact, and activate it with rollback.

## Preconditions

- Access to the official Visa schedule / bulletin for the target countries.
- Ability to create a draft configuration version in the interchange repository.
- Historical sample of transactions for those countries (for simulation).
- Four-eyes approvers identified (scheme ops + finance or risk).

## Procedure

### 1. Source capture

1. Download or export the official fee publication.
2. Record title, publisher, URL or vault path, publication date, effective date.
3. Compute checksum (`SHA-256`) and store under `sources/` for the package.

### 2. Draft country package

1. Create `visa-{CC}-{region}-fees` version `X.Y.Z` (bump minor for rate changes, major for structural rule changes).
2. Set `effective_from` to the network effective date (UTC).
3. Map programs / rules / rates; mark illustrative or internal overlays explicitly.
4. Validate against [`schemas/visa-country-fee-package.schema.json`](schemas/visa-country-fee-package.schema.json).

### 3. Semantic checks

- No overlapping rules with identical priority for the same condition space.
- Consumer IFR caps respected for covered Intra-EEA consumer products (if modeling EEA).
- Domestic vs Intra-EEA vs interregional relations are explicit.
- Currency and rounding policy present.
- Every rate has a `source_reference`.

### 4. Simulation

1. Replay a country-scoped historical sample in **shadow** mode.
2. Compare estimated interchange vs previous package and vs actual settlement when available.
3. Flag large deltas by MCC, product, and channel.
4. Produce a change report for approvers.

### 5. Approval and activation

1. Four-eyes approve the draft.
2. Activate atomically for `country_code` at `effective_from` (or immediate activation time if mid-cycle hotfix and policy allows).
3. Keep prior version readable for historical replay.

### 6. Post-activation

1. Monitor estimation vs actual variance for 7–14 days.
2. Open incidents for unexplained systematic deltas.
3. If needed, roll back by reactivating the previous package version (do not delete history).

## Hotfix vs cycle update

| Type | When | Versioning |
|------|------|------------|
| Cycle update | Scheduled Visa April/October (or regional) publication | Minor/major bump, normal simulation window |
| Hotfix | Mid-cycle bulletin or defect | Patch bump, accelerated simulation, mandatory rollback plan |

## Rollback

1. Identify last-known-good `package_id` + `version`.
2. Activate it with a new activation record (append-only history).
3. Re-run variance dashboards.
4. Quarantine the bad version as `rejected` or `retired`.

## Checklist

- [ ] Official source archived with checksum
- [ ] Schema validation passed
- [ ] Semantic / overlap checks passed
- [ ] Simulation report attached
- [ ] Four-eyes approval recorded
- [ ] Active version pinned on transactions after go-live
- [ ] Monitoring alerts enabled for country variance
