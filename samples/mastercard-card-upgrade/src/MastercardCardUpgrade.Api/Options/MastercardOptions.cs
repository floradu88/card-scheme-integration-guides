namespace MastercardCardUpgrade.Api.Options;

public sealed class MastercardOptions
{
    public const string SectionName = "Mastercard";

    /// <summary>Mastercard API host used for every call. Paths below are appended to this.</summary>
    public string BaseUrl { get; set; } = "https://sandbox.api.mastercard.com";

    /// <summary><c>OAuth1</c> (Mastercard Developers default) or <c>Bearer</c>.</summary>
    public string AuthMode { get; set; } = "OAuth1";

    /// <summary>Bearer token when <see cref="AuthMode"/> is <c>Bearer</c>.</summary>
    public string Token { get; set; } = "";

    public string ConsumerKey { get; set; } = "";
    public string SigningKeyP12Path { get; set; } = "";
    public string SigningKeyAlias { get; set; } = "keyalias";
    public string SigningKeyPassword { get; set; } = "";
    public string PrivateKeyPemPath { get; set; } = "";

    public string SandboxSampleAccountRange { get; set; } = "585240844";

    /// <summary>
    /// <c>Local</c> runs an in-process ACS simulator using the official 3.1.0 field names.
    /// <c>Mastercard</c> calls <see cref="BaseUrl"/> + ACS paths (OAuth/Bearer + optional JWE).
    /// </summary>
    public string AlmMode { get; set; } = "Local";

    public string EncryptionCertificatePath { get; set; } = "";
    public string EncryptionKeyId { get; set; } = "";
    public string DecryptionKeyPath { get; set; } = "";

    public string RequestIdHeader { get; set; } = "Universal-Spec-Api-Request-Id";
    public string CorrelationIdQuery { get; set; } = "correlation_id";

    public int RequestTimeoutSeconds { get; set; } = 30;

    public MastercardPaths Paths { get; set; } = new();

    public bool UseBearerAuth =>
        string.Equals(AuthMode, "Bearer", StringComparison.OrdinalIgnoreCase);

    public bool HasBearerToken => !string.IsNullOrWhiteSpace(Token);

    public bool HasSigningMaterial =>
        !string.IsNullOrWhiteSpace(ConsumerKey)
        && (!string.IsNullOrWhiteSpace(SigningKeyP12Path) || !string.IsNullOrWhiteSpace(PrivateKeyPemPath));

    public bool HasCredentials => UseBearerAuth ? HasBearerToken : HasSigningMaterial;

    public bool UseLiveMastercardAlm =>
        string.Equals(AlmMode, "Mastercard", StringComparison.OrdinalIgnoreCase);

    public bool HasJweMaterial =>
        !string.IsNullOrWhiteSpace(EncryptionCertificatePath)
        && !string.IsNullOrWhiteSpace(DecryptionKeyPath);

    public Uri BaseUri => new(BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);

    public Uri Url(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new InvalidOperationException("Mastercard path is not configured.");

        if (Uri.TryCreate(relativePath, UriKind.Absolute, out var absolute))
            return absolute;

        return new Uri(BaseUri, relativePath.TrimStart('/'));
    }
}

public sealed class MastercardPaths
{
    public string BinLookup { get; set; } = "/bin-ranges/account-searches";
    public string AcsRegistrations { get; set; } = "/asc/acs-api/account-registrations";
    public string AcsDeleteRegistrations { get; set; } = "/asc/acs-api/account-registrations/delete-registrations";
}

public sealed class ProductCatalogOptions
{
    public const string SectionName = "ProductCatalog";

    public List<ProductDefinition> Products { get; set; } = [];
    public List<ProductTransition> AllowedTransitions { get; set; } = [];
}

public sealed class ProductDefinition
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string LineOfBusiness { get; set; } = "";
}

public sealed class ProductTransition
{
    public string From { get; set; } = "";
    public List<string> To { get; set; } = [];
}
