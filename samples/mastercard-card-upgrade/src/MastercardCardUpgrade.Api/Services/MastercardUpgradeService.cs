using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public sealed class MastercardUpgradeService : IMastercardUpgradeService
{
    private readonly ICardStore _store;
    private readonly ICardLifecycleService _lifecycle;
    private readonly MastercardOptions _options;

    public MastercardUpgradeService(
        ICardStore store,
        ICardLifecycleService lifecycle,
        IOptions<MastercardOptions> options)
    {
        _store = store;
        _lifecycle = lifecycle;
        _options = options.Value;
    }

    public async Task<CardUpgradeResult> UpgradeAsync(
        CardUpgradeRequest request,
        CancellationToken cancellationToken = default)
    {
        var pan = PanRules.Normalize(request.Pan);
        PanRules.Validate(pan);

        var card = _store.FindByPan(pan);
        if (card is null)
        {
            var created = await _lifecycle.CreateAsync(
                new CreateCardRequest(
                    request.ServiceCode ?? InferSourceProduct(request.TargetProductCode),
                    pan,
                    LookupBin: _options.HasCredentials),
                cancellationToken);
            card = _store.GetRequired(created.CardId);
        }

        var migration = await _lifecycle.UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(request.TargetProductCode, "CUSTOMER_UPGRADE", request.CorrelationId),
            cancellationToken);

        var latest = _store.GetRequired(card.CardId);
        return new CardUpgradeResult(
            migration.CorrelationId,
            latest.MaskedPan,
            migration.SourceProductCode,
            latest.ProductDescription,
            latest.Ica,
            migration.TargetProductCode,
            migration.Status,
            migration.MastercardRequestId,
            migration.MastercardRawResponse);
    }

    private static string InferSourceProduct(string target) =>
        string.Equals(target, "MCG", StringComparison.OrdinalIgnoreCase) ? "MCW" : "MCG";
}
