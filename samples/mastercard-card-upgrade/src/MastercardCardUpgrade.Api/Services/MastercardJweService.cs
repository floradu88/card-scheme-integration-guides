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
        _config = JweConfigBuilder.AJweEncryptionConfig()
            .WithEncryptionCertificate(certificate)
            .WithDecryptionKey(decryptionKey)
            .WithEncryptionPath("$", "$")
            .WithDecryptionPath("$", "$")
            .WithEncryptedValueFieldName("encryptedValue")
            .Build();
    }

    public bool IsEnabled => _config is not null;

    public string Encrypt(string json) =>
        _config is null ? json : JweEncryption.EncryptPayload(json, _config);

    public string Decrypt(string json) =>
        _config is null ? json : JweEncryption.DecryptPayload(json, _config);
}
