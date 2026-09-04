namespace MastercardCardUpgrade.Api.Services;

/// <summary>
/// ACS write left the issuer: timeout, HTTP 408, or a dropped connection.
/// Do not change local product state and do not retry with the same request id.
/// </summary>
public sealed class AcsAmbiguousOutcomeException : Exception
{
    public string RequestId { get; }
    public string? ResponseBody { get; }

    public AcsAmbiguousOutcomeException(string requestId, string message, string? responseBody = null, Exception? inner = null)
        : base(message, inner)
    {
        RequestId = requestId;
        ResponseBody = PanRedactor.Redact(responseBody);
    }
}

public sealed class IdempotencyConflictException : Exception
{
    public IdempotencyConflictException(string message) : base(message)
    {
    }
}

public sealed class KillSwitchException : InvalidOperationException
{
    public KillSwitchException()
        : base("Mastercard writes are disabled (Mastercard:WritesEnabled=false).")
    {
    }
}
