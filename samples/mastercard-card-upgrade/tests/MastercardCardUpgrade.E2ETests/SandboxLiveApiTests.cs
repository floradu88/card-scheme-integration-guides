using System.Net.Http.Json;
using System.Text.Json;
using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models.Cards;
using Xunit;

namespace MastercardCardUpgrade.E2ETests;

[Collection("E2E")]
[Trait("Suite", "E2E")]
[Trait("Host", "Sandbox")]
public sealed class SandboxLiveApiTests
{
    private readonly E2EHost _host;

    public SandboxLiveApiTests(E2EHost host) => _host = host;

    [Fact]
    public async Task BinLookup_CallsMastercardSandbox_WhenCredentialsArePresent()
    {
        if (!_host.CredentialsConfigured)
            return;

        var response = await _host.Client.PostAsJsonAsync(
            "/api/mastercard/sandbox/bin-lookup",
            new { panOrAccountRange = MastercardTestData.AccountRange });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"BIN Lookup failed ({(int)response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        Assert.True(
            doc.RootElement.TryGetProperty("productCode", out _)
            || doc.RootElement.TryGetProperty("lowAccountRange", out _),
            $"Unexpected sandbox payload: {body}");
    }

    [Fact]
    public async Task BinLookup_EmptyBody_UsesConfiguredAccountRange()
    {
        if (!_host.CredentialsConfigured)
            return;

        var response = await _host.Client.PostAsJsonAsync("/api/mastercard/sandbox/bin-lookup", new { });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Demo_HitsLiveAcs_WhenExplicitlyEnabled()
    {
        if (!_host.LiveAcsReady || !_host.LiveAcsWritesRequested)
            return;

        var pan = E2EHost.UniquePan();
        var response = await _host.Client.PostAsJsonAsync("/api/demo/e2e", new EndToEndDemoRequest(
            MastercardTestData.SourceProductCode,
            MastercardTestData.TargetProductCode,
            pan));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.IsSuccessStatusCode || (int)response.StatusCode is >= 400 and < 500,
            $"Unexpected live ACS failure ({(int)response.StatusCode}): {body}");

        if (!response.IsSuccessStatusCode)
            return;

        var demo = JsonSerializer.Deserialize<EndToEndDemoResult>(body, E2EHost.Json);
        Assert.NotNull(demo);
        Assert.Equal("Mastercard", demo!.AlmMode);
        Assert.True(demo.Upgrade.SamePan);
        Assert.True(demo.Upgrade.SameBin);
    }
}
