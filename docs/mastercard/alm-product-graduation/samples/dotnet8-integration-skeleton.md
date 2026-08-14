# .NET 8 Integration Skeleton

Recommended solution:

```text
src/
  CardProductMigration.Api/
  CardProductMigration.Application/
  CardProductMigration.Domain/
  CardProductMigration.Infrastructure/
    Mastercard/
      Alm/
        Generated/
        MastercardAlmAdapter.cs
        MastercardAlmClient.cs
        MastercardAlmVerificationClient.cs
        MastercardAlmOptions.cs
        MastercardSigningHandler.cs
        MastercardErrorMapper.cs

tests/
  CardProductMigration.UnitTests/
  Mastercard.Alm.ContractTests/
  Mastercard.Alm.SandboxTests/
  Mastercard.Alm.CertificationTests/
```

Network-neutral contract:

```csharp
public interface ICardProductNetworkAdapter
{
    Task<NetworkProductMigrationResult> MoveProductAsync(
        NetworkProductMigration migration,
        CancellationToken ct);

    Task<NetworkProductState> GetProductStateAsync(
        string cardId,
        CancellationToken ct);
}
```

Mastercard adapter:

```csharp
public sealed class MastercardAlmAdapter : ICardProductNetworkAdapter
{
    private readonly IMastercardAlmClient _client;
    private readonly IMastercardAlmVerificationClient _verification;
    private readonly ICardRepository _cards;
    private readonly IProductMappingRepository _mapping;

    public async Task<NetworkProductMigrationResult> MoveProductAsync(
        NetworkProductMigration migration,
        CancellationToken ct)
    {
        var card = await _cards.GetRequiredAsync(migration.CardId, ct);
        var target = await _mapping.GetMastercardTargetAsync(
            migration.TargetProgramId, ct);

        ValidateAllowedRange(card, target);
        ValidateAllowedTransition(card.CurrentProgramId, migration.TargetProgramId);

        var before = await _verification.GetStateAsync(card.PanReference, ct);

        var result = await _client.GraduateProductAsync(
            card.PanReference,
            target.MastercardProductCode,
            ct);

        var after = await _verification.GetStateAsync(card.PanReference, ct);

        if (after.ProductCode != target.MastercardProductCode)
            return NetworkProductMigrationResult.Unknown(result.CorrelationId);

        return NetworkProductMigrationResult.Completed(result.CorrelationId);
    }
}
```

Important: `GraduateProductAsync` and `GetStateAsync` must be implemented using the exact methods/types generated or bound from the ACS contract provisioned to the real Mastercard project. Do not serialize guessed fields from this skeleton.
