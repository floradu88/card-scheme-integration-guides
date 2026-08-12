# 2. Dynamic fees per country

Generated: 2026-08-12

## 2.1 Why country packages

Mastercard fees are not a single global table. Domestic rates, Intra-EEA multilateral fees, and interregional fees can all apply depending on issuer, merchant, and acquirer geography. Operations therefore need **dynamic, country-scoped fee packages** that can be imported, validated, simulated, and activated independently.

Recommended model:

```text
MastercardFeeCatalog (network=MASTERCARD)
  -> CountryPackage[ISO-3166 country]
       -> metadata (version, effective_from/to, source_refs)
       -> region_relations (DOMESTIC, INTRA_EEA, INTERREGIONAL, ...)
       -> programs / rules / rates
       -> country_overrides (MCC, channel, product)
```

A transaction selects packages using merchant country (and related issuer/acquirer geography), then evaluates rules inside the matching package version that was **active at clearing time**.

## 2.2 Country keying rules

| Field | Rule |
|-------|------|
| `country_code` | ISO 3166-1 alpha-2 (e.g. `RO`, `DE`, `FR`) |
| `region` | Logical market bucket (`EEA`, `UK`, `US`, `APAC`, ...) |
| `currency` | ISO 4217 default settlement/display currency for the package |
| `package_id` | Stable ID: `mastercard-{country}-{region}-fees` |
| `version` | SemVer for the package contents |

Do **not** overwrite an active package in place. Publish a new version with a new `effective_from`.

## 2.3 Dynamic update triggers

Update a country package when any of the following occur:

1. Mastercard publishes a new fee / interchange PDF or bulletin for that market;
2. Domestic regulators or IFR-related guidance changes applicable caps;
3. Product launch (new commercial program, contactless threshold, sector rate);
4. Brexit / territory / membership changes that alter region relations;
5. Internal commercial overlay or estimation policy change (kept separate from network rates).

## 2.4 Resolution algorithm (runtime)

```text
inputs: merchant_country, issuer_country, acquirer_country, product, channel, mcc, amount, event_time
1. Resolve region_relation(merchant, issuer, acquirer)
2. Load active Mastercard CountryPackage for merchant_country at event_time
   (fallback: region default package if country-specific missing and policy allows)
3. Match highest-priority rule where all conditions hold
4. Compute fee = percentage(amount) + fixed, apply rounding / min / max / caps
5. Persist: package_id, version, rule_id, program_id, computed_fee, region_relation
```

Authorization-time estimates may use a provisional package; clearing-time re-qualification must use the package effective for the clearing event and must retain the exact version ID on the transaction record.

## 2.5 Separation of concerns

| Layer | Owns |
|-------|------|
| Network country package | Published Mastercard schedules for that country / relation |
| Region fallback package | Intra-EEA / interregional multilateral defaults |
| Org overlay | Internal estimation adjustments, never claimed as network-actual |
| Actual interchange from settlement | Ground truth for reconciliation |

## 2.6 Compatibility with shared interchange packs

Country packages should compile into (or map onto) the shared interchange configuration model:

- [`../../interchange/configuration-pack/`](../../interchange/configuration-pack/)
- [`../../interchange/phase2-engineering-handbook/`](../../interchange/phase2-engineering-handbook/)
- [`../../platform/07-dynamic-interchange-engine/`](../../platform/07-dynamic-interchange-engine/)

Use `network: MASTERCARD` and include `merchant_country` / `issuer_country` conditions in rules so multi-country catalogs remain queryable in one engine alongside Visa packages.
