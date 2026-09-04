namespace MastercardCardUpgrade.Api.Models.Cards;

public enum CardStatus
{
    Issued,
    Registered,
    Closed
}

public enum MigrationStatus
{
    Created,
    Submitted,
    Accepted,
    Rejected,
    Active,
    RolledBack,
    Reconciling
}

public sealed class CardAccount
{
    public required string CardId { get; init; }
    public required string Pan { get; set; }
    public required string MaskedPan { get; set; }
    public required string Bin { get; set; }
    public required string ProductCode { get; set; }
    public string? ProductDescription { get; set; }
    public string? Ica { get; set; }
    public string? ExpiryMmYy { get; set; }
    public CardStatus Status { get; set; } = CardStatus.Issued;
    public string? AcsProductRuleId { get; set; }
    public string? LastAcsRequestId { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ProductMigration
{
    public required string MigrationId { get; init; }
    public required string CardId { get; init; }
    public required string SourceProductCode { get; init; }
    public required string TargetProductCode { get; init; }
    public required string Reason { get; init; }
    public required string CorrelationId { get; init; }
    public MigrationStatus Status { get; set; } = MigrationStatus.Created;
    public string? MastercardRequestId { get; set; }
    public string? ProductRuleId { get; set; }
    public object? MastercardRawResponse { get; set; }
    public string? FailureReason { get; set; }
    public bool SamePan { get; set; } = true;
    public bool SameBin { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed record CreateCardRequest(
    string ProductCode,
    string? Pan = null,
    string? ExpiryMmYy = null,
    bool LookupBin = true);

public sealed record CardResponse(
    string CardId,
    string MaskedPan,
    string Bin,
    string ProductCode,
    string? ProductDescription,
    string? Ica,
    string Status,
    string? AcsProductRuleId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpgradeCardRequest(
    string TargetProductCode,
    string? Reason = null,
    string? CorrelationId = null);

public sealed record MigrationResponse(
    string MigrationId,
    string CardId,
    string MaskedPan,
    string Bin,
    string SourceProductCode,
    string TargetProductCode,
    string Status,
    string CorrelationId,
    string? MastercardRequestId,
    string? ProductRuleId,
    bool SamePan,
    bool SameBin,
    string AlmMode,
    object? MastercardRawResponse,
    string? FailureReason);

public sealed record EndToEndDemoRequest(
    string? SourceProductCode = null,
    string? TargetProductCode = null,
    string? Pan = null);

public sealed record EndToEndDemoResult(
    CardResponse Card,
    MigrationResponse Registration,
    MigrationResponse Upgrade,
    string AlmMode,
    string Summary);
