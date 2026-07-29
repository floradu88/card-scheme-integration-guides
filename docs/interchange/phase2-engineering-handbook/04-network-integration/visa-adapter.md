# Visa Adapter

## Publicly documented integration surface

VisaNet Connect Acceptance exposes RESTful connectivity for approved acquirers, acquirer-processors and technology partners. Public documentation describes ISO 20022/ATICA naming, sandbox access, mutual TLS, and transaction APIs.

Official links:

- https://developer.visa.com/capabilities/visanet-connect-acceptance
- https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-getting-started
- https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-how-to
- https://developer.visa.com/capabilities/visanet-connect-acceptance/docs-authentication
- https://developer.visa.com/capabilities/visanet-connect-acceptance/reference

## Adapter responsibilities

- map Visa API or legacy message fields to normalized attributes;
- preserve Visa program/network indicators;
- derive fallback, authentication and tokenization flags;
- map clearing outcomes and actual interchange;
- attach source/parser versions.

## Important limitation

The public API reference may expose field names and examples, but production program identifiers, full technical specifications, private interchange qualification guides and certification material can require authorized Visa access. Do not infer undocumented mappings from field names alone.

## Suggested interface

```csharp
public interface IVisaTransactionAdapter
{
    NormalizedTransaction MapAuthorization(VisaAuthorization input);
    NormalizedTransaction MapClearing(VisaClearingRecord input);
    ActualInterchange MapActual(VisaSettlementRecord input);
}
```
