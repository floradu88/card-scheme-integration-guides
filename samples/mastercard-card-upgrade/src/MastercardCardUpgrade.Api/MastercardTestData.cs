namespace MastercardCardUpgrade.Api;

/// <summary>
/// Public sandbox/Postman examples used by the POC.
/// BIN Lookup: Mastercard Developers collection (accountRange 585240844).
/// PAN/upgrade: local Card Upgrade collection + ACS 3.1.0 swagger examples (MCW, 00616, request id).
/// </summary>
public static class MastercardTestData
{
    public const string AccountRange = "585240844";
    public const string Pan = "5555555555554444";
    public const string MaskedPan = "555555******4444";
    public const string ExpiryMmYy = "1223";
    public const string SourceProductCode = "MCG";
    public const string SwaggerProductCode = "MCW";
    public const string TargetProductCode = "MWE";
    public const string BinLookupProductCode = "MCO";
    public const string AlmServiceCode = "00616";
    public const string RequestId = "21ad4488-615b-4004-6158-ec5abff7d58f";
    public const string ProductRuleId = "4eb71aff-34fa-401d-9001-0790526293ca";
    public const string EffectiveDate = "2026-09-04";
    public const string AccountIndicator = "PRIMARY_ACCOUNT_NUMBER";
    public const string LocalBaseUrl = "http://localhost:5088";
}
