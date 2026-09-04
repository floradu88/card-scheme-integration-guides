using System.Collections.Concurrent;
using MastercardCardUpgrade.Api.Models.Acs;

namespace MastercardCardUpgrade.Api.Services;

/// <summary>
/// In-process ACS 3.1.0 stand-in so create → register → upgrade can run without
/// issuer encryption keys. Uses the same field names as the official swagger.
/// </summary>
public sealed class LocalAcsClient : IAcsClient
{
    private readonly ConcurrentDictionary<string, AcsAccountRegistration> _byPan = new();
    private readonly ConcurrentDictionary<string, AcsOperationResult> _byRequestId = new();

    public string Mode => "Local";

    public Task<AcsOperationResult> RegisterPanAsync(
        string pan,
        string productCode,
        string? expiryMmYy,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        if (_byPan.ContainsKey(pan))
            throw new EligibilityException("PAN is already registered in Product Graduation Plus.");

        var ruleId = Guid.NewGuid().ToString();
        var interimBody = BuildBody(pan, productCode, expiryMmYy, ruleId, AcsResponseTypes.Interim);
        var finalBody = BuildBody(pan, productCode, expiryMmYy, ruleId, AcsResponseTypes.Final);
        _byPan[pan] = interimBody;
        var result = ToResult(requestId, interimBody, AcsResponseTypes.Interim);
        _byRequestId[requestId] = ToResult(requestId, finalBody, AcsResponseTypes.Final) with { Status = "ACTIVE" };
        return Task.FromResult(result);
    }

    public Task<AcsOperationResult> UpdatePanProductAsync(
        string pan,
        string productCode,
        string? productRuleId,
        string? expiryMmYy,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        if (!_byPan.TryGetValue(pan, out var existing))
            throw new EligibilityException("PAN is not registered in Product Graduation Plus. Register it first.");

        var ruleId = productRuleId
                     ?? existing.AccountLevelManagement?.ProductRules.FirstOrDefault()?.ProductRuleId
                     ?? Guid.NewGuid().ToString();

        var interimBody = BuildBody(pan, productCode, expiryMmYy, ruleId, AcsResponseTypes.Interim);
        var finalBody = BuildBody(pan, productCode, expiryMmYy, ruleId, AcsResponseTypes.Final);
        _byPan[pan] = interimBody;
        var result = ToResult(requestId, interimBody, AcsResponseTypes.Interim);
        _byRequestId[requestId] = ToResult(requestId, finalBody, AcsResponseTypes.Final) with { Status = "ACTIVE" };
        return Task.FromResult(result);
    }

    public Task<AcsOperationResult> GetStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        if (_byRequestId.TryGetValue(requestId, out var result))
            return Task.FromResult(result);

        throw new KeyNotFoundException($"No ACS request '{requestId}'.");
    }

    private static AcsAccountRegistration BuildBody(
        string pan,
        string productCode,
        string? expiryMmYy,
        string ruleId,
        string responseType) =>
        new()
        {
            AccountIdentifier = pan,
            AccountIndicator = AcsAccountIndicators.PrimaryAccountNumber,
            AccountIdentifierExpirationDate = expiryMmYy,
            AccountLevelManagement = new AcsAccountLevelManagement
            {
                AccountIdentifierStatus = "A",
                ProductRules =
                [
                    new AcsProductRule
                    {
                        ProductRuleId = ruleId,
                        ProductRuleStatus = "A",
                        ProductGraduationProductCode = productCode,
                        ResponseIndicator = AcsResponseIndicators.Accepted,
                        ResponseType = responseType,
                        AlmServiceCode = "00616",
                        AlmServiceProductCode = productCode,
                        AlmServiceProductClass = productCode,
                        AccountCategoryCode = "P",
                        ActionCodes = responseType == AcsResponseTypes.Final
                            ? ["A0000101", "A0000128"]
                            : [],
                        ServiceClassificationCodes = ["PRGR"],
                        RejectCodes = []
                    }
                ]
            }
        };

    private static AcsOperationResult ToResult(
        string requestId,
        AcsAccountRegistration body,
        string responseType)
    {
        var rule = body.AccountLevelManagement!.ProductRules[0];
        return new AcsOperationResult(
            true,
            requestId,
            "SUBMITTED",
            rule.ProductRuleId,
            rule.ProductGraduationProductCode,
            rule.ResponseIndicator ?? AcsResponseIndicators.Accepted,
            responseType,
            body);
    }
}
