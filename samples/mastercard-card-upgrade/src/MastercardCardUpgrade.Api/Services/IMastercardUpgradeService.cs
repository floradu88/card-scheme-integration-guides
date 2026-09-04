using MastercardCardUpgrade.Api.Models;

namespace MastercardCardUpgrade.Api.Services;

public interface IMastercardUpgradeService
{
    Task<CardUpgradeResult> UpgradeAsync(
        CardUpgradeRequest request,
        CancellationToken cancellationToken = default);
}
