namespace MastercardCardUpgrade.Api.Models;

public sealed record CardUpgradeRequest(
    string Pan,
    string TargetProductCode,
    DateOnly? EffectiveDate = null,
    string? ServiceCode = null,
    string? CorrelationId = null);

public sealed record CardUpgradeResult(
    string CorrelationId,
    string MaskedPan,
    string? CurrentProductCode,
    string? CurrentProductDescription,
    string? Ica,
    string TargetProductCode,
    string SubmissionStatus,
    string? MastercardReference,
    object? MastercardRawResponse);

public sealed record AlmSubmission(
    string Pan,
    string TargetProductCode,
    string? Ica,
    DateOnly EffectiveDate,
    string? ServiceCode,
    string CorrelationId);

public sealed record AlmSubmissionResult(
    bool Accepted,
    string? MastercardReference,
    string Status,
    object? RawResponse);

public sealed record AlmStatusResult(
    string Status,
    object? RawResponse);
