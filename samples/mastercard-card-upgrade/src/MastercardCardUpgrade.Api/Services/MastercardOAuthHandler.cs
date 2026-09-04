using System.Net.Http.Headers;
using Mastercard.Developer.OAuth1Signer.Core.Signers;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

/// <summary>
/// Authenticates outbound Mastercard API calls from configuration:
/// Bearer token, or OAuth 1.0a RSA-SHA256 (oauth_body_hash).
/// </summary>
public sealed class MastercardOAuthHandler : DelegatingHandler
{
    private readonly MastercardSigningKeyHolder _keys;
    private readonly MastercardOptions _options;

    public MastercardOAuthHandler(
        MastercardSigningKeyHolder keys,
        IOptions<MastercardOptions> options)
    {
        _keys = keys;
        _options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri is null || !request.RequestUri.IsAbsoluteUri)
            request.RequestUri = _options.Url(request.RequestUri?.OriginalString ?? "");

        if (request.Content is not null)
            await request.Content.LoadIntoBufferAsync();

        if (_options.UseBearerAuth)
        {
            if (string.IsNullOrWhiteSpace(_options.Token))
                throw new InvalidOperationException(
                    "Mastercard:Token is not configured. Set AuthMode=Bearer and Token in appsettings.Local.json.");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
        }
        else
        {
            var signer = new NetHttpClientSigner(_keys.ConsumerKey, _keys.GetSigningKey());
            await signer.SignAsync(request);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
