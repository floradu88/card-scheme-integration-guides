# Official Mastercard website references

Curated links and facts from Mastercard public websites. Prefer these over secondary summaries when building adapters, fee packages, or certification plans. Always archive the exact edition (PDF/HTML), publication date, effective date, and SHA-256 checksum used in production.

**Last reviewed from public sites:** 2026-08-12

## 1. Portals

| Portal | URL | Use for |
|--------|-----|---------|
| Mastercard Developers | https://developer.mastercard.com/ | API products, sandbox projects, keys, OpenAPI specs |
| Platform documentation | https://developer.mastercard.com/platform/documentation/ | Auth, encryption, getting started |
| Mastercard Connect | https://www.mastercardconnect.com/ | Customer portal: publications, Key Management Portal (KMP), company apps |
| Business rules (US) | https://www.mastercard.com/us/en/business/support/rules.html | Public rules downloads |
| Business rules (Europe) | https://www.mastercard.com/europe/en/business/support/rules.html | Merchant/processor-facing rules & compliance |

## 2. Mastercard Developers — getting started & security

| Topic | URL |
|-------|-----|
| Quick start guide | https://developer.mastercard.com/platform/documentation/getting-started-with-mastercard-apis/quick-start-guide/ |
| Getting started with Mastercard APIs | https://developer.mastercard.com/platform/documentation/getting-started-with-mastercard-apis/ |
| Using OAuth 1.0a | https://developer.mastercard.com/platform/documentation/security-and-authentication/using-oauth-1a-to-access-mastercard-apis/ |
| Payload encryption | https://developer.mastercard.com/platform/documentation/security-and-authentication/securing-sensitive-data-using-payload-encryption/ |
| Creating keys (project dashboard) | Documented under platform security/auth; create Consumer Key + PKCS#12 signing key in project |
| Support / status | Linked from developer.mastercard.com footer (Support, Forum, Status) |

### Authentication model (from Mastercard Developers docs)

- Mastercard Developers APIs authenticate clients with **OAuth 1.0a** (including body-hash extension for signed requests).
- Each request is signed with an **RSA private key**; Mastercard verifies with the registered public key.
- Project setup yields a **Consumer Key** and a **signing key** (typically downloaded as a password-protected PKCS#12).
- Keep private keys in a vault or HSM; never commit them.

### Payload encryption (from Mastercard Developers docs)

- Separate from OAuth: encrypt sensitive fields (PCI / PII) when the product requires it.
- Schemes documented by Mastercard include **Field Level Encryption** and **JWE**.
- Project setup commonly provides a public request-encryption certificate and a private response-decryption key.

### Official open-source client libraries

| Library | URL |
|---------|-----|
| OAuth1 signer (C#) | https://github.com/Mastercard/oauth1-signer-csharp |
| OAuth1 signer (Java) | https://github.com/Mastercard/oauth1-signer-java |
| OAuth1 signer (Node.js) | https://github.com/Mastercard/oauth1-signer-nodejs |
| OAuth1 signer (Go) | https://github.com/Mastercard/oauth1-signer-go |
| Client encryption (Java) | https://github.com/Mastercard/client-encryption-java |
| Client encryption (Go) | https://github.com/Mastercard/client-encryption-go |
| API client tutorial | https://github.com/Mastercard/mastercard-api-client-tutorial |

For .NET, prefer the language samples Mastercard publishes for the selected product, or a security-reviewed OAuth 1.0a / JOSE implementation aligned with the product guide.

## 3. Acquiring / transaction APIs (examples)

Product availability is project- and role-specific. Confirm access in the Developers catalog and with your Mastercard / acquirer sponsor.

| Product | URL |
|---------|-----|
| Transaction API for Acquirers (docs) | https://developer.mastercard.com/transaction-api-for-acquirers/documentation/ |
| Transaction API reference app | https://github.com/Mastercard/transaction-api-reference-app |
| Key Management Portal (via Connect) | https://www.mastercardconnect.com/ (search Store for Key Management Portal) |

When mTLS client certificates are required (product-dependent), Mastercard documents obtaining them through **Key Management Portal (KMP)** inside Mastercard Connect.

## 4. Interchange fee hubs (public)

| Region | Hub | Notes from site |
|--------|-----|-----------------|
| United States | https://www.mastercard.com/us/en/business/support/merchant-interchange-rates.html | Domestic US + interregional rates for US merchants; Merchant Category Guide, criteria, glossary |
| Europe | https://www.mastercard.com/europe/en/business/support/merchant-interchange-rates.html | Intra-EEA, interregional, and **per-country / intra-location** POS fee PDFs |
| United Kingdom (Europe hub) | https://www.mastercard.com/gb/en/business/support/merchant-interchange-rates.html | UK domestic treatment under UK IFR variant |
| Canada | https://www.mastercard.com/ca/en/business/support/merchant-interchange-rates.html | Canada domestic + interregional |

### Facts published on Mastercard US interchange hub

- Interchange is generally paid by **acquirers to issuers** on purchase transactions.
- It is **one component** of the Merchant Discount Rate (MDR); Mastercard states it has no involvement in acquirer–merchant pricing agreements.
- Qualification depends on criteria such as merchant category, auth-to-clear timing, magstripe presence, enhanced data, and volume tiers.
- Rates are typically updated **semiannually**; the website may lag the official rate set—**official rates prevail** on discrepancy.
- Example published schedule (verify currency before use): [2025–2026 U.S. Region Interchange Programs and Rates (PDF)](https://www.mastercard.com/content/dam/mccom/us/business/documents/merchant-rates-2025-2026.pdf) (effective **11 Apr 2025** per document title).

### Facts published on Mastercard Europe interchange hub

- Interchange is described as a small fee typically paid by the retailer’s bank (acquirer) to the cardholder’s bank (issuer).
- **IFR** caps apply to EEA domestic and EEA cross-border **consumer** debit/credit; the **UK** version caps **UK domestic** consumer debit/credit.
- Intra-EEA fallback POS fees and **country/location-specific PDFs** are linked from the hub (including Romania, Germany, UK, etc.).
- Mastercard EEA subregion (as published for intra-EEA fee application) includes EU Member States, Iceland, Liechtenstein, Norway, and Andorra (for transactions with those countries)—confirm current list on the hub before modeling.

## 5. Public rules PDFs

| Document | URL |
|----------|-----|
| Mastercard Rules | https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/mastercard-rules.pdf |
| Transaction Processing Rules | https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/transaction-processing-rules.pdf |
| Mastercard Switch Rules | https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/mastercard-switch-rules-manual.pdf |
| Security Rules and Procedures | https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/SPME-Manual.pdf |
| Chargeback Guide | https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/chargeback-guide.pdf |
| EU regulations context | https://www.mastercard.com/europe/en/for-the-world/about-us/eu-regulations.html |

## 6. How to use these sources in this repo

| Work item | Primary sources |
|-----------|-----------------|
| Country fee package import | US / Europe / CA / country PDF from hubs §4 |
| Dynamic fee runbook | Semiannual US cadence + Europe “changes published promptly” language |
| API client auth | OAuth 1.0a guide + official signer libraries §2 |
| Sensitive field handling | Payload encryption guide + client-encryption libraries §2 |
| Network adapter mapping | Transaction Processing Rules + product API docs §3 / §5 |
| Certification / keys | Mastercard Connect + KMP + Developers project credentials |

## 7. Documentation boundary

Public websites and PDFs do **not** replace:

- Mastercard Connect customer publications and private bulletins;
- participant / CID-specific manuals;
- certification scripts and test cards;
- settlement file layouts from your processor or Mastercard;
- commercial overlays negotiated with acquirers.

Record every production import in the source registry (`network`, title, edition, effective date, URL/vault path, checksum, owner).

## Related packs in this repository

- Fee modeling: [`interchange-fees/`](interchange-fees/)
- Quick start: [`quick-start/`](quick-start/)
- Security add-on: [`mastercard-connect-security/`](mastercard-connect-security/)
- NFR link index: [`nfr-security-operations/13-official-references/mastercard-links.md`](nfr-security-operations/13-official-references/mastercard-links.md)
- Adapter source index: [`adapter/official-source-index.md`](adapter/official-source-index.md)
