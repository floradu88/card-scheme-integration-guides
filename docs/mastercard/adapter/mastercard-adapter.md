# Mastercard Adapter

## Official public foundation

- Mastercard Rules: https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/mastercard-rules.pdf
- Transaction Processing Rules: https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/transaction-processing-rules.pdf
- Mastercard Switch Rules: https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/mastercard-switch-rules-manual.pdf
- Security Rules and Procedures: https://www.mastercard.com/content/dam/mccom/shared/business/support/rules-pdfs/SPME-Manual.pdf
- Europe interchange hub: https://www.mastercard.com/europe/en/business/support/merchant-interchange-rates.html
- US interchange hub: https://www.mastercard.com/us/en/business/support/merchant-interchange-rates.html

## Adapter responsibilities

- parse authorization and clearing records from the processor/network interface;
- map product, merchant, POS, authentication and timing attributes;
- preserve network-assigned program/rate identifiers;
- map actual interchange and settlement outputs;
- track parser and source versions.

## Important limitation

Detailed Mastercard message layouts, interchange manuals and portal publications may require Mastercard Connect or customer-specific access. Public rule documents define broad processing obligations but are not a complete implementation specification.

## Suggested interface

```csharp
public interface IMastercardTransactionAdapter
{
    NormalizedTransaction MapAuthorization(McAuthorization input);
    NormalizedTransaction MapClearing(McClearingRecord input);
    ActualInterchange MapActual(McSettlementRecord input);
}
```
