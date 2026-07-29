# Card Scheme Integration Guides

Documentation packs for payment platform engineering and card-scheme integration. Content is organized by **card network** and by **shared platform topic**. Source ZIP archives are kept under [`archives/`](archives/).

Start here: [`docs/README.md`](docs/README.md)

## By card scheme

| Scheme | Docs | Notes |
|--------|------|-------|
| **Visa** | [`docs/visa/`](docs/visa/) | Onboarding, adapter, six integration options (A-F), NFR / security / operations |
| **Mastercard** | [`docs/mastercard/`](docs/mastercard/) | Onboarding and interchange adapter notes (thinner coverage than Visa) |
| Other schemes | — | No Amex / Discover / UnionPay / JCB packs in this corpus |

## By topic

| Topic | Path |
|-------|------|
| Docs hub | [`docs/`](docs/) |
| Payment Platform Architecture Handbook v3 (shared) | [`docs/platform/`](docs/platform/) |
| Dynamic interchange (Visa + Mastercard) | [`docs/interchange/`](docs/interchange/) |
| Original ZIP archives | [`archives/`](archives/) |

## Recommended reading order (platform)

1. [`docs/platform/00-master-index/`](docs/platform/00-master-index/)
2. [`docs/platform/01-foundation/`](docs/platform/01-foundation/)
3. [`docs/platform/02-network-integration/`](docs/platform/02-network-integration/) — includes Visa and Mastercard onboarding
4. [`docs/platform/03-security-compliance/`](docs/platform/03-security-compliance/)
5. [`docs/platform/04-platform-engineering/`](docs/platform/04-platform-engineering/)
6. [`docs/platform/05-operations/`](docs/platform/05-operations/)
7. [`docs/platform/06-engineering-assets/`](docs/platform/06-engineering-assets/)

## Visa quick start

1. [`docs/visa/onboarding/`](docs/visa/onboarding/)
2. Choose an option under [`docs/visa/integration-options/`](docs/visa/integration-options/)
3. Read [`docs/visa/nfr-security-operations/`](docs/visa/nfr-security-operations/) for NFR, monitoring, security, and runbooks
4. Adapter / interchange: [`docs/visa/adapter/`](docs/visa/adapter/), [`docs/interchange/`](docs/interchange/)
5. Interchange fees and dynamic per-country updates: [`docs/visa/interchange-fees/`](docs/visa/interchange-fees/)

## Mastercard quick start

1. [`docs/mastercard/onboarding/`](docs/mastercard/onboarding/)
2. [`docs/mastercard/adapter/`](docs/mastercard/adapter/)
3. Shared network / clearing material in [`docs/platform/02-network-integration/`](docs/platform/02-network-integration/)
4. Interchange design in [`docs/interchange/`](docs/interchange/)

## Archives note

Standalone ZIPs `A-authorization-only-poc.zip` through `F-processor-connection-replacement.zip` are byte-identical to the folders under `docs/visa/integration-options/`. They are retained in `archives/` for provenance; day-to-day browsing should use the extracted tree.
