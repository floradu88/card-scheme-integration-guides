using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MastercardCardUpgrade.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MastercardCardUpgrade.SandboxTests;

public sealed class SandboxFixture : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public JsonSerializerOptions SerializerOptions => JsonOptions;

    public Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Sandbox");
        });
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }
}

[CollectionDefinition("Sandbox")]
public sealed class SandboxCollection : ICollectionFixture<SandboxFixture>
{
}

[Collection("Sandbox")]
public sealed class SandboxStatusTests
{
    private readonly SandboxFixture _fixture;

    public SandboxStatusTests(SandboxFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Status_ReturnsSandboxConfiguration()
    {
        var response = await _fixture.Client.GetAsync("/api/mastercard/sandbox/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("https://sandbox.api.mastercard.com", json.GetProperty("baseUrl").GetString());
        Assert.True(json.TryGetProperty("credentialsConfigured", out _));
        Assert.Contains("/bin-ranges/account-searches", json.GetProperty("binLookupUrl").GetString());
    }
}

[Collection("Sandbox")]
public sealed class SandboxBinLookupTests
{
    private readonly SandboxFixture _fixture;

    public SandboxBinLookupTests(SandboxFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task BinLookup_CallsMastercardSandbox_WhenCredentialsArePresent()
    {
        var status = await _fixture.Client.GetFromJsonAsync<SandboxStatusResponse>(
            "/api/mastercard/sandbox/status",
            _fixture.SerializerOptions);
        Assert.NotNull(status);

        if (!status.CredentialsConfigured)
            return;

        var response = await _fixture.Client.PostAsJsonAsync(
            "/api/mastercard/sandbox/bin-lookup",
            new { panOrAccountRange = "585240844" });

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.IsSuccessStatusCode,
            $"Mastercard sandbox BIN Lookup failed ({(int)response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.True(
            doc.RootElement.TryGetProperty("productCode", out _)
            || doc.RootElement.TryGetProperty("lowAccountRange", out _),
            $"Unexpected sandbox payload: {body}");
    }
}
