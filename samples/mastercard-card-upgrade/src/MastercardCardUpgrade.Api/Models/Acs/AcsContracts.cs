using System.Text.Json.Serialization;

namespace MastercardCardUpgrade.Api.Models.Acs;

/// <summary>
/// Account Catalog Services API 3.1.0 contracts from
/// https://static.developer.mastercard.com/content/account-catalog-services/swagger/acs-api-swagger.yaml
/// </summary>
public sealed class AcsAccountRegistration
{
    [JsonPropertyName("accountIdentifier")]
    public string AccountIdentifier { get; set; } = "";

    [JsonPropertyName("accountIndicator")]
    public string AccountIndicator { get; set; } = AcsAccountIndicators.PrimaryAccountNumber;

    [JsonPropertyName("accountIdentifierExpirationDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountIdentifierExpirationDate { get; set; }

    [JsonPropertyName("accountLevelManagement")]
    public AcsAccountLevelManagement? AccountLevelManagement { get; set; }
}

public sealed class AcsAccountLevelManagement
{
    [JsonPropertyName("accountIdentifierStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountIdentifierStatus { get; set; }

    [JsonPropertyName("productRules")]
    public List<AcsProductRule> ProductRules { get; set; } = [];
}

public sealed class AcsProductRule
{
    [JsonPropertyName("productRuleId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProductRuleId { get; set; }

    [JsonPropertyName("productRuleStatus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProductRuleStatus { get; set; }

    [JsonPropertyName("productGraduationProductCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProductGraduationProductCode { get; set; }

    [JsonPropertyName("groupId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GroupId { get; set; }

    [JsonPropertyName("responseIndicator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResponseIndicator { get; set; }

    [JsonPropertyName("responseType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResponseType { get; set; }

    [JsonPropertyName("almServiceCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlmServiceCode { get; set; }

    [JsonPropertyName("almServiceProductCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlmServiceProductCode { get; set; }

    [JsonPropertyName("almServiceProductClass")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AlmServiceProductClass { get; set; }

    [JsonPropertyName("accountCategoryCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountCategoryCode { get; set; }

    [JsonPropertyName("rejectCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? RejectCodes { get; set; }

    [JsonPropertyName("actionCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ActionCodes { get; set; }

    [JsonPropertyName("serviceClassificationCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ServiceClassificationCodes { get; set; }
}

public static class AcsAccountIndicators
{
    public const string PrimaryAccountNumber = "PRIMARY_ACCOUNT_NUMBER";
}

public static class AcsResponseIndicators
{
    public const string Accepted = "ACCEPTED";
    public const string Rejected = "REJECTED";
    public const string Pending = "PENDING";
}

public static class AcsResponseTypes
{
    public const string Interim = "INTERIM";
    public const string Final = "FINAL";
}

public sealed class AcsAccountDelete
{
    [JsonPropertyName("accountIdentifier")]
    public string AccountIdentifier { get; set; } = "";

    [JsonPropertyName("accountLevelManagement")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AcsAccountLevelManagement? AccountLevelManagement { get; set; }
}

public sealed record AcsOperationResult(
    bool Accepted,
    string RequestId,
    string Status,
    string? ProductRuleId,
    string? ProductCode,
    string ResponseIndicator,
    string ResponseType,
    object RawResponse);
