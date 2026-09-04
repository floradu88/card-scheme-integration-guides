using System.Net;
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

    public Task<AcsOperationResult> DeleteRegistrationAsync(
        string pan,
        string? productRuleId,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        var payload = new AcsAccountDelete
        {
            AccountIdentifier = pan,
            AccountLevelManagement = string.IsNullOrWhiteSpace(productRuleId)
                ? null
                : new AcsAccountLevelManagement
                {
                    ProductRules = [new AcsProductRule { ProductRuleId = productRuleId }]
                }
        };

        var url = _options.Url(_options.Paths.AcsDeleteRegistrations);
        var separator = string.IsNullOrEmpty(url.Query) ? "?" : "&";
        var pathWithService = $"{url}{separator}services=ALM";
        return SendAsync(HttpMethod.Post, pathWithService, requestId, payload, cancellationToken, absolute: true);
    }

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

        using var response = await SendProtectedAsync(request, requestId, cancellationToken);
        var raw = await ReadBodyAsync(response, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new MastercardApiException("ACS status", (int)response.StatusCode, response.ReasonPhrase, raw);

        return Parse(requestId, raw);
    }

    private Task<AcsOperationResult> SendAsync(
        HttpMethod method,
        string path,
        string requestId,
        object payload,
        CancellationToken cancellationToken,
        bool absolute = false)
    {
        EnsureLiveEncryption();
        return SendCoreAsync(method, path, requestId, payload, cancellationToken, absolute);
    }

    private async Task<AcsOperationResult> SendCoreAsync(
        HttpMethod method,
        string path,
        string requestId,
        object payload,
        CancellationToken cancellationToken,
        bool absolute)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        if (_jwe.IsEnabled)
            json = _jwe.Encrypt(json);

        var uri = absolute ? new Uri(path, UriKind.Absolute) : _options.Url(path);
        using var request = new HttpRequestMessage(method, uri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.TryAddWithoutValidation(_options.RequestIdHeader, requestId);

        using var response = await SendProtectedAsync(request, requestId, cancellationToken);
        var raw = await ReadBodyAsync(response, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new MastercardApiException("ACS account-registrations", (int)response.StatusCode, response.ReasonPhrase, raw);

        return Parse(requestId, raw);
    }

    private async Task<HttpResponseMessage> SendProtectedAsync(
        HttpRequestMessage request,
        string requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _http.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                var raw = await ReadBodyAsync(response, cancellationToken);
                throw new AcsAmbiguousOutcomeException(
                    requestId,
                    "ACS returned HTTP 408. Do not retry with the same Universal-Spec-Api-Request-Id; GET by correlation_id.",
                    raw);
            }

            return response;
        }
        catch (AcsAmbiguousOutcomeException)
        {
            throw;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AcsAmbiguousOutcomeException(
                requestId,
                "ACS request timed out before a response was received. Query GET ?correlation_id= before any retry.",
                inner: ex);
        }
        catch (HttpRequestException ex)
        {
            throw new AcsAmbiguousOutcomeException(
                requestId,
                "ACS connection dropped after the write may have left the issuer. Query GET ?correlation_id= before any retry.",
                inner: ex);
        }
    }

    private void EnsureLiveEncryption()
    {
        if (_options.UseLiveMastercardAlm && !_jwe.IsEnabled)
            throw new InvalidOperationException(
                "ACS payloads are x-mastercard-api-encrypted. Set Mastercard:EncryptionCertificatePath and DecryptionKeyPath (and EncryptionKeyId if the portal shows a fingerprint).");
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
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new AcsOperationResult(
                true,
                requestId,
                "SUBMITTED",
                null,
                null,
                AcsResponseIndicators.Accepted,
                AcsResponseTypes.Interim,
                new { });
        }

        var body = JsonSerializer.Deserialize<AcsAccountRegistration>(raw, JsonOptions)
                   ?? throw new InvalidOperationException("Empty ACS response.");
        var rule = body.AccountLevelManagement?.ProductRules.FirstOrDefault();
        var indicator = rule?.ResponseIndicator ?? AcsResponseIndicators.Accepted;
        var accepted = !indicator.Equals(AcsResponseIndicators.Rejected, StringComparison.OrdinalIgnoreCase);

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
