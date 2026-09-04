using System.Text;
using System.Text.Json;
using MastercardCardUpgrade.Api.Models.Acs;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public sealed class MastercardAcsClient : IAcsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly MastercardOptions _options;
    private readonly MastercardJweService _jwe;

    public MastercardAcsClient(
        HttpClient http,
        IOptions<MastercardOptions> options,
        MastercardJweService jwe)
    {
        _http = http;
        _options = options.Value;
        _jwe = jwe;
        _http.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
        _http.BaseAddress ??= _options.BaseUri;
    }

    public string Mode => "Mastercard";

    public Task<AcsOperationResult> RegisterPanAsync(
        string pan,
        string productCode,
        string? expiryMmYy,
        string requestId,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpMethod.Post,
            _options.Paths.AcsRegistrations,
            requestId,
            BuildRegistration(pan, productCode, expiryMmYy),
            cancellationToken);

    public Task<AcsOperationResult> UpdatePanProductAsync(
        string pan,
        string productCode,
        string? productRuleId,
        string? expiryMmYy,
        string requestId,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpMethod.Put,
            _options.Paths.AcsRegistrations,
            requestId,
            BuildRegistration(pan, productCode, expiryMmYy, productRuleId),
            cancellationToken);

    public async Task<AcsOperationResult> GetStatusAsync(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        var url = _options.Url(_options.Paths.AcsRegistrations);
        var separator = string.IsNullOrEmpty(url.Query) ? "?" : "&";
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{url}{separator}{Uri.EscapeDataString(_options.CorrelationIdQuery)}={Uri.EscapeDataString(requestId)}");
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await _http.SendAsync(request, cancellationToken);
        var raw = await ReadBodyAsync(response, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new MastercardApiException("ACS status", (int)response.StatusCode, response.ReasonPhrase, raw);

        return Parse(requestId, raw);
    }

    private async Task<AcsOperationResult> SendAsync(
        HttpMethod method,
        string path,
        string requestId,
        AcsAccountRegistration payload,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        if (_jwe.IsEnabled)
            json = _jwe.Encrypt(json);

        using var request = new HttpRequestMessage(method, _options.Url(path))
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.TryAddWithoutValidation(_options.RequestIdHeader, requestId);

        using var response = await _http.SendAsync(request, cancellationToken);
        var raw = await ReadBodyAsync(response, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new MastercardApiException("ACS account-registrations", (int)response.StatusCode, response.ReasonPhrase, raw);

        return Parse(requestId, raw);
    }

    private async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        return _jwe.IsEnabled ? _jwe.Decrypt(raw) : raw;
    }

    private static AcsAccountRegistration BuildRegistration(
        string pan,
        string productCode,
        string? expiryMmYy,
        string? productRuleId = null) =>
        new()
        {
            AccountIdentifier = pan,
            AccountIndicator = AcsAccountIndicators.PrimaryAccountNumber,
            AccountIdentifierExpirationDate = expiryMmYy,
            AccountLevelManagement = new AcsAccountLevelManagement
            {
                AccountIdentifierStatus = "A",
                ProductRules =
                [
                    new AcsProductRule
                    {
                        ProductRuleId = productRuleId,
                        ProductRuleStatus = "A",
                        ProductGraduationProductCode = productCode
                    }
                ]
            }
        };

    private static AcsOperationResult Parse(string requestId, string raw)
    {
        var body = JsonSerializer.Deserialize<AcsAccountRegistration>(raw, JsonOptions)
                   ?? throw new InvalidOperationException("Empty ACS response.");
        var rule = body.AccountLevelManagement?.ProductRules.FirstOrDefault();
        var indicator = rule?.ResponseIndicator ?? AcsResponseIndicators.Accepted;
        var accepted = indicator.Equals(AcsResponseIndicators.Accepted, StringComparison.OrdinalIgnoreCase);

        return new AcsOperationResult(
            accepted,
            requestId,
            accepted ? "SUBMITTED" : "REJECTED",
            rule?.ProductRuleId,
            rule?.ProductGraduationProductCode ?? rule?.AlmServiceProductCode,
            indicator,
            rule?.ResponseType ?? AcsResponseTypes.Interim,
            body);
    }
}
