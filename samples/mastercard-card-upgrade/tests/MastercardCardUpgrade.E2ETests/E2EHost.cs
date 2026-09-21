using System.Net.Http.Json;
using System.Text.Json;
using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MastercardCardUpgrade.E2ETests;

/// <summary>
/// Dual-mode HTTP host: in-process WebApplicationFactory (default) or a running API
/// at CARD_UPGRADE_E2E_BASE_URL (for example http://localhost:5088 with the Sandbox profile).
/// </summary>
public sealed class E2EHost : IAsyncLifetime
{
    public const string BaseUrlEnv = "CARD_UPGRADE_E2E_BASE_URL";
    public const string LiveAcsEnv = "CARD_UPGRADE_E2E_LIVE_ACS";

    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;
    public SandboxStatusResponse Status { get; private set; } = null!;
    public bool IsExternal { get; private set; }

    public bool CredentialsConfigured => Status.CredentialsConfigured;
    public bool LiveAcsReady => Status.LiveAcsReady;
    public bool LiveAcsWritesRequested =>
        string.Equals(Environment.GetEnvironmentVariable(LiveAcsEnv), "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable(LiveAcsEnv), "true", StringComparison.OrdinalIgnoreCase);

    public bool UsesLiveMastercardAlm =>
        string.Equals(Status.AlmMode, "Mastercard", StringComparison.OrdinalIgnoreCase);

    public bool ShouldSkipLiveAcsWrites() => UsesLiveMastercardAlm && !LiveAcsWritesRequested;

    public void SkipIfLiveAcsWritesAreNotOptedIn()
    {
        if (ShouldSkipLiveAcsWrites())
            throw new InvalidOperationException(
                $"Refusing ACS writes while AlmMode=Mastercard. Set {LiveAcsEnv}=1 to opt in.");
    }

    public static string UniquePan() => PanRules.GenerateMastercardTestPan(MastercardTestData.Pan);
    public static string UniqueRequestId() => Guid.NewGuid().ToString();

    public Task InitializeAsync()
    {
        var baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnv);
        IsExternal = !string.IsNullOrWhiteSpace(baseUrl);

        if (IsExternal)
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl!.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(60)
            };
        }
        else
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Sandbox");
                builder.UseSetting("Mastercard:CardStorePath", "");
                builder.UseSetting("Mastercard:ReconcileIntervalSeconds", "0");
            });
            Client = _factory.CreateClient();
        }

        return LoadStatusAsync();
    }

    public async Task LoadStatusAsync()
    {
        var response = await Client.GetAsync("/api/mastercard/sandbox/status");
        Status = await ReadOk<SandboxStatusResponse>(response);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    public static WebApplicationFactory<Program> IsolatedFactory(params (string Key, string Value)[] settings)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Sandbox");
            builder.UseSetting("Mastercard:CardStorePath", "");
            builder.UseSetting("Mastercard:ReconcileIntervalSeconds", "0");
            foreach (var (key, value) in settings)
                builder.UseSetting(key, value);
        });
    }

    public static async Task<T> ReadOk<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        var value = JsonSerializer.Deserialize<T>(body, Json);
        Assert.NotNull(value);
        return value!;
    }

    public async Task<CardResponse> CreateCardAsync(
        string? pan = null,
        string productCode = MastercardTestData.SourceProductCode,
        bool lookupBin = false)
    {
        var response = await Client.PostAsJsonAsync("/api/cards", new
        {
            productCode,
            pan = pan ?? UniquePan(),
            expiryMmYy = MastercardTestData.ExpiryMmYy,
            lookupBin
        });
        return await ReadOk<CardResponse>(response);
    }

    public async Task<MigrationResponse> RegisterAsync(string cardId, string? correlationId = null)
    {
        SkipIfLiveAcsWritesAreNotOptedIn();
        var url = correlationId is null
            ? $"/api/cards/{cardId}/register"
            : $"/api/cards/{cardId}/register?correlationId={Uri.EscapeDataString(correlationId)}";
        return await ReadOk<MigrationResponse>(await Client.PostAsync(url, null));
    }

    public async Task<MigrationResponse> UpgradeAsync(string cardId, string target, string? reason = null, string? correlationId = null)
    {
        SkipIfLiveAcsWritesAreNotOptedIn();
        var response = await Client.PostAsJsonAsync(
            $"/api/cards/{cardId}/upgrades",
            new { targetProductCode = target, reason, correlationId });
        return await ReadOk<MigrationResponse>(response);
    }
}

[CollectionDefinition("E2E", DisableParallelization = true)]
public sealed class E2ECollection : ICollectionFixture<E2EHost>
{
}
