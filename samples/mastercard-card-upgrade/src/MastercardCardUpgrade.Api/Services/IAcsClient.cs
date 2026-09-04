using MastercardCardUpgrade.Api.Models.Acs;

namespace MastercardCardUpgrade.Api.Services;

public interface IAcsClient
{
    string Mode { get; }

    Task<AcsOperationResult> RegisterPanAsync(
        string pan,
        string productCode,
        string? expiryMmYy,
        string requestId,
        CancellationToken cancellationToken = default);

    Task<AcsOperationResult> UpdatePanProductAsync(
        string pan,
        string productCode,
        string? productRuleId,
        string? expiryMmYy,
        string requestId,
        CancellationToken cancellationToken = default);

    Task<AcsOperationResult> GetStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default);
}
