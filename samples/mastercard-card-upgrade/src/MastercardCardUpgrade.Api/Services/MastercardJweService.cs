using Mastercard.Developer.ClientEncryption.Core.Encryption;
using Mastercard.Developer.ClientEncryption.Core.Encryption.JWE;
using Mastercard.Developer.ClientEncryption.Core.Utils;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public sealed class MastercardJweService
{
    private readonly JweConfig? _config;

    public MastercardJweService(IOptions<MastercardOptions> options)
    {
        var cfg = options.Value;
        if (!cfg.HasJweMaterial)
            return;

        var certificate = EncryptionUtils.LoadEncryptionCertificate(cfg.EncryptionCertificatePath);
        var decryptionKey = EncryptionUtils.LoadDecryptionKey(cfg.DecryptionKeyPath);
        var builder = JweConfigBuilder.AJweEncryptionConfig()
            .WithEncryptionCertificate(certificate)
            .WithDecryptionKey(decryptionKey)
            .WithEncryptionPath(
                string.IsNullOrWhiteSpace(cfg.JweEncryptionPath) ? "$" : cfg.JweEncryptionPath,
                string.IsNullOrWhiteSpace(cfg.JweEncryptionOutPath) ? "$" : cfg.JweEncryptionOutPath)
            .WithDecryptionPath(
                string.IsNullOrWhiteSpace(cfg.JweDecryptionPath) ? "$" : cfg.JweDecryptionPath,
                string.IsNullOrWhiteSpace(cfg.JweDecryptionOutPath) ? "$" : cfg.JweDecryptionOutPath)
            .WithEncryptedValueFieldName(
                string.IsNullOrWhiteSpace(cfg.EncryptedValueFieldName) ? "encryptedValue" : cfg.EncryptedValueFieldName);

        if (!string.IsNullOrWhiteSpace(cfg.EncryptionKeyId))
            builder = builder.WithEncryptionKeyFingerprint(cfg.EncryptionKeyId);

        _config = builder.Build();
    }

    public bool IsEnabled => _config is not null;

    public string Encrypt(string json) =>
        _config is null ? json : JweEncryption.EncryptPayload(json, _config);

    public string Decrypt(string json)
    {
        if (_config is null || string.IsNullOrWhiteSpace(json) || !LooksEncrypted(json))
            return json;

        return JweEncryption.DecryptPayload(json, _config);
    }

    private static bool LooksEncrypted(string json) =>
        json.Contains("encryptedValue", StringComparison.OrdinalIgnoreCase)
        || json.Contains("encryptedData", StringComparison.OrdinalIgnoreCase);
}
