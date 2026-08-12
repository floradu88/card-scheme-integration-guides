# Card Scheme Integration Guides

Documentation packs for payment platform engineering and card-scheme integration. Content is organized by **card network** and by **shared platform topic**. Source ZIP archives are kept under [`archives/`](archives/).

Start here: [`docs/README.md`](docs/README.md)

## By card scheme

| Scheme | Docs | Notes |
|--------|------|-------|
| **Visa** | [`docs/visa/`](docs/visa/) | Quick start, onboarding, adapter, interchange fees, options A-F, NFR / security / ops, VisaNet Connect |
| **Mastercard** | [`docs/mastercard/`](docs/mastercard/) | Same layout as Visa: quick start, options A-F, NFR / ops, Connect security, fees, certification |
| Other schemes | — | No Amex / Discover / UnionPay / JCB packs in this corpus |

## By topic

| Topic | Path |
|-------|------|
| Docs hub | [`docs/`](docs/) |
| Payment Platform Architecture Handbook v3 (shared) | [`docs/platform/`](docs/platform/) |
| Dynamic interchange engine (phase 07) | [`docs/platform/07-dynamic-interchange-engine/`](docs/platform/07-dynamic-interchange-engine/) |
| Dynamic interchange (Visa + Mastercard) | [`docs/interchange/`](docs/interchange/) |
| Interchange addons (existing-project extension) | [`docs/interchange/addons/`](docs/interchange/addons/) |
| Platform addons (generic DI requirements) | [`docs/platform/addons/`](docs/platform/addons/) |
| Visa interchange fees (per country) | [`docs/visa/interchange-fees/`](docs/visa/interchange-fees/) |
| Mastercard interchange fees (per country) | [`docs/mastercard/interchange-fees/`](docs/mastercard/interchange-fees/) |
| Original ZIP archives | [`archives/`](archives/) |

## Recommended reading order (platform)

1. [`docs/platform/00-master-index/`](docs/platform/00-master-index/)
2. [`docs/platform/01-foundation/`](docs/platform/01-foundation/)
3. [`docs/platform/02-network-integration/`](docs/platform/02-network-integration/) — includes Visa and Mastercard onboarding
4. [`docs/platform/03-security-compliance/`](docs/platform/03-security-compliance/)
5. [`docs/platform/04-platform-engineering/`](docs/platform/04-platform-engineering/)
6. [`docs/platform/05-operations/`](docs/platform/05-operations/)
7. [`docs/platform/06-engineering-assets/`](docs/platform/06-engineering-assets/)
8. [`docs/platform/07-dynamic-interchange-engine/`](docs/platform/07-dynamic-interchange-engine/)
9. [`docs/platform/08-visa-developer-certification/`](docs/platform/08-visa-developer-certification/)
10. [`docs/platform/09-mastercard-developer-certification/`](docs/platform/09-mastercard-developer-certification/)

## Visa quick start

1. [`docs/visa/quick-start/`](docs/visa/quick-start/)
2. [`docs/visa/onboarding/`](docs/visa/onboarding/)
3. Choose an option under [`docs/visa/integration-options/`](docs/visa/integration-options/)
4. Read [`docs/visa/nfr-security-operations/`](docs/visa/nfr-security-operations/) for NFR, monitoring, security, and runbooks
5. VisaNet Connect security: [`docs/visa/visanet-connect-security/`](docs/visa/visanet-connect-security/)
6. Adapter / interchange: [`docs/visa/adapter/`](docs/visa/adapter/), [`docs/interchange/`](docs/interchange/)
7. Interchange fees and dynamic per-country updates: [`docs/visa/interchange-fees/`](docs/visa/interchange-fees/)

## Mastercard quick start

1. [`docs/mastercard/quick-start/`](docs/mastercard/quick-start/)
2. [`docs/mastercard/onboarding/`](docs/mastercard/onboarding/)
3. Choose an option under [`docs/mastercard/integration-options/`](docs/mastercard/integration-options/)
4. Read [`docs/mastercard/nfr-security-operations/`](docs/mastercard/nfr-security-operations/) for NFR, monitoring, security, and runbooks
5. Mastercard Connect / Developers security: [`docs/mastercard/mastercard-connect-security/`](docs/mastercard/mastercard-connect-security/)
6. Adapter / interchange: [`docs/mastercard/adapter/`](docs/mastercard/adapter/), [`docs/interchange/`](docs/interchange/)
7. Interchange fees and dynamic per-country updates: [`docs/mastercard/interchange-fees/`](docs/mastercard/interchange-fees/)
8. Certification pack: [`docs/platform/09-mastercard-developer-certification/`](docs/platform/09-mastercard-developer-certification/)

## Archives note

Standalone ZIPs `A-authorization-only-poc.zip` through `F-processor-connection-replacement.zip` are byte-identical to the folders under `docs/visa/integration-options/`. Mastercard option packs under `docs/mastercard/integration-options/` are in-repo adaptations (no separate Mastercard A–F ZIPs). Prefer the extracted trees for day-to-day browsing.
