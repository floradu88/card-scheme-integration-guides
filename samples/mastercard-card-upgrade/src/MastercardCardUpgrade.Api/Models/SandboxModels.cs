namespace MastercardCardUpgrade.Api.Models;

public sealed record SandboxStatusResponse(
    string Environment,
    string BaseUrl,
    string AuthMode,
    bool CredentialsConfigured,
    bool SigningKeyFileFound,
    string SigningKeyKind,
    bool AlmSubmissionConfigured,
    string NextStep)
{
    public string AlmMode { get; init; } = "Local";
    public string? BinLookupUrl { get; init; }
    public string? AcsRegistrationsUrl { get; init; }
    public string? AcsDeleteRegistrationsUrl { get; init; }
    public bool WritesEnabled { get; init; } = true;
    public bool JweConfigured { get; init; }
    public bool LiveAcsReady { get; init; }
    public string? CardStorePath { get; init; }
}
