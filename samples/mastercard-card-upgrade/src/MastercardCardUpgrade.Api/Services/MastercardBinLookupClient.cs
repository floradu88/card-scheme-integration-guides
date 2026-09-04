using System.Text;
using System.Text.Json;
using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public sealed class MastercardBinLookupClient : IMastercardBinLookupClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly MastercardOptions _options;

    public MastercardBinLookupClient(HttpClient http, IOptions<MastercardOptions> options)
    {
        _http = http;
        _options = options.Value;
        _http.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
    }

    public async Task<BinAccountRangeResponse> SearchAccountRangeAsync(
        string panOrAccountRange,
        CancellationToken cancellationToken = default)
    {
        var digits = new string(panOrAccountRange.Where(char.IsDigit).ToArray());
        if (digits.Length < 6)
            throw new ArgumentException("At least 6 digits are required.");

        // Mastercard public Postman docs state 6-8 digit BINs or up to
        // the 11th digit of an account range.
        var lookupDigits = digits[..Math.Min(11, digits.Length)];
        if (!long.TryParse(lookupDigits, out var accountRange))
            throw new ArgumentException("The account range is invalid.");

        var json = JsonSerializer.Serialize(new AccountRangeSearchRequest(accountRange));

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Url(_options.Paths.BinLookup))
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new MastercardApiException(
                "Mastercard BIN Lookup",
                (int)response.StatusCode,
                response.ReasonPhrase,
                body);

        var result = JsonSerializer.Deserialize<BinAccountRangeResponse>(body, JsonOptions);
        return result ?? throw new InvalidOperationException("Mastercard returned an empty BIN Lookup response.");
    }
}
