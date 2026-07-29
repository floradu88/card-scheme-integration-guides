# 1. Visa interchange fees overview

Generated: 2026-07-29

## 1.1 What interchange is

Interchange is the fee paid between acquiring and issuing participants for a cleared Visa transaction. It is **not** the full merchant discount rate. Merchant cost typically includes:

1. interchange (network-qualified program and rate);
2. scheme / assessment fees;
3. acquirer or processor markup and services.

This pack focuses on **interchange qualification and fee configuration**, especially when rates differ by country or market.

## 1.2 Fee axes that change the rate

Visa qualification commonly depends on combinations of:

| Axis | Examples |
|------|----------|
| Geography | Domestic, intra-regional (e.g. Intra-EEA), interregional |
| Merchant / acquirer country | ISO 3166-1 alpha-2 country of merchant outlet / acquirer |
| Issuer country / region | Card issuing country or region relation |
| Product | Consumer debit/credit/prepaid, commercial, premium |
| Channel | Card-present, contactless, e-commerce, MOTO |
| Authentication / security | Secure e-commerce, non-secure, EMV chip |
| MCC / merchant category | Standard vs sector programs (fuel, grocery, government, etc.) |
| Timing | Authorization vs clearing re-qualification |

Exact program identifiers and private qualification matrices may require Visa Online / participant manuals.

## 1.3 Regulatory caps (illustrative context)

In the European Economic Area (EEA), the Interchange Fee Regulation (IFR) caps **consumer** card interchange for covered intra-EEA transactions at:

- **0.20%** consumer debit / prepaid (regulated context);
- **0.30%** consumer credit / deferred debit (regulated context).

Commercial, corporate, and many premium products are typically **outside** those consumer caps and vary by program, country, and channel. Domestic schedules can differ from Intra-EEA multilateral fees. UK / EEA cross-border treatment changed after Brexit and must be modeled as distinct region relations.

Treat the percentages above as regulatory context, not as a complete fee table.

## 1.4 Public reference hubs

Always prefer the official schedule for the market and effective date you are importing:

- [Visa Rules and Policies](https://www.visa.com/en-us/support/visa-rules)
- [Visa Core Rules and Visa Product and Service Rules (PDF)](https://usa.visa.com/dam/VCOM/download/about-visa/visa-rules-public.pdf)
- [Visa Merchant Fees and Interchange](https://usa.visa.com/support/small-business/regulations-fees.html)
- Visa Europe / UK fee and interchange PDF publications (example Intra-EEA pack):  
  `https://www.visa.co.uk/content/dam/VCOM/regional/ve/unitedkingdom/PDF/fees-and-interchange/`

Publication cadence for public rate sheets is often aligned to April / October cycles, but emergency or regional updates can land outside that cadence.

## 1.5 Documentation boundary

Public material does **not** replace:

- licensed Visa interchange manuals;
- regional domestic supplements;
- Visa Business News / bulletins;
- processor field mappings for actual assessed interchange;
- organization-specific commercial overlays.

Store a source index (title, URL or vault path, publication date, effective date, checksum) with every imported country package.
