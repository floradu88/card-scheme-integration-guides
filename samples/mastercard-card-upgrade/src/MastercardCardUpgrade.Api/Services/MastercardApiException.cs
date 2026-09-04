namespace MastercardCardUpgrade.Api.Services;

public sealed class MastercardApiException : Exception
{
    public int StatusCode { get; }
    public string? ResponseBody { get; }
    public string Operation { get; }

    public MastercardApiException(string operation, int statusCode, string? reason, string? responseBody)
        : base($"{operation} failed: {statusCode} {reason}. Body: {responseBody}")
    {
        Operation = operation;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
