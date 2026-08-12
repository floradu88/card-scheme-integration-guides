# 1. Mastercard interchange fees overview

Generated: 2026-08-12

## 1.1 What interchange is

Interchange is the fee paid between acquiring and issuing participants for a cleared Mastercard transaction. It is **not** the full merchant discount rate. Merchant cost typically includes:

1. interchange (network-qualified program and rate);
2. scheme / assessment fees;
3. acquirer or processor markup and services.

This pack focuses on **interchange qualification and fee configuration**, especially when rates differ by country or market.

## 1.2 Fee axes that change the rate

Mastercard qualification commonly depends on combinations of:

| Axis | Examples |
|------|----------|
| Geography | Domestic, Intra-EEA / intra-regional, interregional |
| Merchant / acquirer country | ISO 3166-1 alpha-2 country of merchant outlet / acquirer |
| Issuer country / region | Card issuing country or region relation |
| Product | Consumer debit/credit/prepaid, commercial, World / World Elite and other premium |
| Channel | Card-present, contactless, e-commerce, MO/TO |
| Authentication / security | Secure e-commerce, non-secure, EMV chip |
| MCC / merchant category | Standard vs sector programs (fuel, grocery, government, etc.) |
| Timing | Authorization vs clearing re-qualification |

Exact IRD / program identifiers and private qualification matrices may require Mastercard Connect or customer implementation manuals.

## 1.3 Regulatory caps (illustrative context)

In the European Economic Area (EEA), the Interchange Fee Regulation (IFR) caps **consumer** card interchange for covered intra-EEA transactions at:

- **0.20%** consumer debit / prepaid (regulated context);
- **0.30%** consumer credit / deferred debit (regulated context).

Commercial, corporate, and many premium products are typically **outside** those consumer caps and vary by program, country, and channel. Domestic schedules can differ from Intra-EEA multilateral fees. UK / EEA cross-border treatment changed after Brexit and must be modeled as distinct region relations.

Treat the percentages above as regulatory context, not as a complete fee table.

## 1.4 Public reference hubs

Always prefer the official schedule for the market and effective date you are importing:

- [Mastercard Rules hub](https://www.mastercard.com/us/en/business/support/rules.html)
- [Mastercard Rules (PDF)](https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/mastercard-rules.pdf)
- [Transaction Processing Rules (PDF)](https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/transaction-processing-rules.pdf)
- [Europe interchange hub](https://www.mastercard.com/europe/en/business/support/merchant-interchange-rates.html)
- [US interchange hub](https://www.mastercard.com/us/en/business/support/merchant-interchange-rates.html)
- [EU regulations context](https://www.mastercard.com/europe/en/for-the-world/about-us/eu-regulations.html)

Publication cadence for public rate sheets varies by region; emergency or regional updates can land outside the usual cycle.

## 1.5 Documentation boundary

Public material does **not** replace:

- licensed Mastercard interchange manuals;
- regional domestic supplements;
- Mastercard Connect / customer bulletins;
- processor field mappings for actual assessed interchange;
- organization-specific commercial overlays.

Store a source index (title, URL or vault path, publication date, effective date, checksum) with every imported country package.
