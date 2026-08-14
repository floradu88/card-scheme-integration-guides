# Official Visa website references

Curated links and facts from Visa public websites (Visa Developer + Visa.com). Archive the exact edition used for each production package.

**Last reviewed from public sites:** 2026-08-12

## 1. Portals

| Portal | URL | Use for |
|--------|-----|---------|
| Visa Developer | https://developer.visa.com/ | Projects, sandbox, credentials, API docs |
| VisaNet Connect – Acceptance | https://developer.visa.com/capabilities/visanet-connect-acceptance | Direct VisaNet acceptance APIs |
| Visa Rules portal | https://www.visa.com/en-us/support/visa-rules | Public rules |
| Merchant fees / interchange (US) | https://usa.visa.com/support/small-business/regulations-fees.html | Fee / IRF context |

## 2. VisaNet Connect – Acceptance

| Topic | URL |
|-------|-----|
| Product overview | https://developer.visa.com/capabilities/visanet-connect-acceptance |
| Getting started | https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-getting-started |
| How-to | https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-how-to |
| Authentication | https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-authentication |
| API reference | https://developer.visa.com/capabilities/visanet-connect-acceptance/reference |
| Docs hub | https://developer.visa.com/capabilities/visanet-connect-acceptance/docs |

### Facts from Visa Developer (VisaNet Connect – Acceptance)

- Enables acquirers, acquirer-processors, and approved technology partners to process in-store, in-app, and online payments over the public internet to VisaNet (REST, ISO 20022 / ATICA naming).
- **Authorization API** can be used in Sandbox by developers; the **full suite** in Sandbox / Certification / Production requires pre-approval by acquirer and Visa (Production Onboarding).
- **Two-Way SSL (mutual authentication)** is required: client and server validate certificates during the TLS handshake.
- **Message Level Encryption (MLE)** is required for Visa Payments Processing API implementations to protect sensitive data (e.g. PAN, cardholder name/address).
- MLE (Visa encryption guide): AES-GCM (128/256) for payload, RSA-OAEP (2048) for key encryption, JWE-based; **separate** Visa server encryption key pair and client encryption key pair for request vs response legs.
- Obtain client certificates via CSR through the Visa Developer project; configure Two-Way SSL keystores (JKS / PKCS#12) per the Two-Way SSL guide.

## 3. Platform security guides

| Topic | URL |
|-------|-----|
| Visa Developer Quick Start | https://developer.visa.com/pages/working-with-visa-apis/visa-developer-quick-start-guide |
| Working with Visa APIs | https://developer.visa.com/pages/working-with-visa-apis |
| Two-Way SSL | https://developer.visa.com/pages/working-with-visa-apis/two-way-ssl |
| Message Level Encryption | https://developer.visa.com/pages/encryption_guide |
| Outbound callback configuration | https://developer.visa.com/pages/working-with-visa-apis/outbound-configuration |
| Visa Developer Center Playground | https://developer.visa.com/pages/visa-developer-center-playground |

## 4. Public rules and fee publications

| Document / hub | URL |
|----------------|-----|
| Visa Rules portal | https://www.visa.com/en-us/support/visa-rules |
| Visa Core Rules PDF (public) | https://usa.visa.com/dam/VCOM/download/about-visa/visa-rules-public.pdf |
| Merchant Data Standards Manual | https://usa.visa.com/dam/VCOM/download/merchants/visa-merchant-data-standards-manual.pdf |
| U.S. Interchange Reimbursement Fees | https://usa.visa.com/dam/VCOM/download/merchants/visa-usa-interchange-reimbursement-fees.pdf |
| Merchant fees & regulations | https://usa.visa.com/support/small-business/regulations-fees.html |
| Visa Business News Digest | https://www.visa.com/en-us/resources/visa-merchant-business-news-digest |

## 5. Documentation boundary

Public Visa Developer and visa.com material does not replace Visa Online, participant manuals, certification scripts, settlement specs, or acquirer agreements.

## Related packs

- Fee modeling: [`interchange-fees/`](interchange-fees/)
- Quick start: [`quick-start/`](quick-start/)
- Security add-on: [`visanet-connect-security/`](visanet-connect-security/)
- NFR link index: [`nfr-security-operations/13-official-references/visa-links.md`](nfr-security-operations/13-official-references/visa-links.md)
