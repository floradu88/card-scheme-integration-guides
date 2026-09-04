using MastercardCardUpgrade.Api.Models;

namespace MastercardCardUpgrade.Api.Services;

public interface IMastercardBinLookupClient
{
    Task<BinAccountRangeResponse> SearchAccountRangeAsync(
        string panOrAccountRange,
        CancellationToken cancellationToken = default);
}
