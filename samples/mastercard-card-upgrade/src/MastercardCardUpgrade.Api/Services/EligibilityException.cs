namespace MastercardCardUpgrade.Api.Services;

public sealed class EligibilityException : Exception
{
    public EligibilityException(string message) : base(message)
    {
    }
}
