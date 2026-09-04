using System.Text.Json.Serialization;

namespace MastercardCardUpgrade.Api.Models;

public sealed record AccountRangeSearchRequest(
    [property: JsonPropertyName("accountRange")] long AccountRange);

public sealed class BinAccountRangeResponse
{
    public long? LowAccountRange { get; set; }
    public long? HighAccountRange { get; set; }
    public string? AcceptanceBrand { get; set; }
    public string? Ica { get; set; }
    public string? CustomerName { get; set; }
    public CountryInfo? Country { get; set; }
    public bool? LocalUse { get; set; }
    public bool? AuthorizationOnly { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductDescription { get; set; }
    public bool? GovernmentRange { get; set; }
    public bool? NonReloadableIndicator { get; set; }
    public string? AnonymousPrepaidIndicator { get; set; }
    public string? ProgramName { get; set; }
    public string? Vertical { get; set; }
    public string? FundingSource { get; set; }
    public string? ConsumerType { get; set; }
    public string? CardholderCurrencyIndicator { get; set; }
}

public sealed class CountryInfo
{
    public string? Code { get; set; }
    public string? Alpha3 { get; set; }
    public string? Name { get; set; }
}
