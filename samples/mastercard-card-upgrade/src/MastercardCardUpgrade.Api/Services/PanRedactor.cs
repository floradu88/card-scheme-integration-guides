using System.Text.Json;
using System.Text.RegularExpressions;

namespace MastercardCardUpgrade.Api.Services;

public static class PanRedactor
{
    private static readonly Regex PanLike = new(@"\d{13,19}", RegexOptions.Compiled);

    public static string? Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return PanLike.Replace(value, match => PanRules.Mask(match.Value));
    }

    public static object? RedactPayload(object? payload)
    {
        if (payload is null)
            return null;

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var redacted = Redact(json);
            return JsonSerializer.Deserialize<JsonElement>(redacted!);
        }
        catch (JsonException)
        {
            return Redact(payload.ToString());
        }
    }
}
