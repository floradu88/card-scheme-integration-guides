using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Mastercard.Developer.OAuth1Signer.Core.Utils;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

/// <summary>
/// Loads the Mastercard sandbox/production signing key once (PKCS#12 preferred, PEM fallback).
/// </summary>
public sealed class MastercardSigningKeyHolder : IDisposable
{
    private readonly MastercardOptions _options;
    private readonly object _gate = new();
    private RSA? _key;

    public MastercardSigningKeyHolder(IOptions<MastercardOptions> options)
    {
        _options = options.Value;
    }

    public RSA GetSigningKey()
    {
        if (_key is not null)
            return _key;

        lock (_gate)
        {
            _key ??= Load();
            return _key;
        }
    }

    public string ConsumerKey
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_options.ConsumerKey))
                throw new InvalidOperationException(
                    "Mastercard:ConsumerKey is not configured. Set ConsumerKey (OAuth1) or AuthMode=Bearer and Token in appsettings.Local.json.");
            return _options.ConsumerKey;
        }
    }

    private RSA Load()
    {
        if (!string.IsNullOrWhiteSpace(_options.SigningKeyP12Path))
        {
            if (!File.Exists(_options.SigningKeyP12Path))
                throw new FileNotFoundException(
                    "Mastercard PKCS#12 signing key (.p12) was not found.",
                    _options.SigningKeyP12Path);

            if (string.IsNullOrWhiteSpace(_options.SigningKeyPassword))
                throw new InvalidOperationException(
                    "Mastercard:SigningKeyPassword is required when using a .p12 keystore.");

            return AuthenticationUtils.LoadSigningKey(
                       _options.SigningKeyP12Path,
                       string.IsNullOrWhiteSpace(_options.SigningKeyAlias) ? "keyalias" : _options.SigningKeyAlias,
                       _options.SigningKeyPassword,
                       X509KeyStorageFlags.EphemeralKeySet)
                   ?? throw new InvalidOperationException("The PKCS#12 file did not contain an RSA private key.");
        }

        if (string.IsNullOrWhiteSpace(_options.PrivateKeyPemPath))
            throw new InvalidOperationException(
                "Configure Mastercard:SigningKeyP12Path (recommended) or Mastercard:PrivateKeyPemPath.");

        if (!File.Exists(_options.PrivateKeyPemPath))
            throw new FileNotFoundException(
                "Mastercard private key PEM was not found.",
                _options.PrivateKeyPemPath);

        var pem = File.ReadAllText(_options.PrivateKeyPemPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return rsa;
    }

    public void Dispose() => _key?.Dispose();
}
