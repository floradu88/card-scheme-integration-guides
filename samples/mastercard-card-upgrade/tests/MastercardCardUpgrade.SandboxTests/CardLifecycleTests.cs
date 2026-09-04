using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MastercardCardUpgrade.Api.Models.Cards;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MastercardCardUpgrade.SandboxTests;

public sealed class CardLifecycleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client;

    public CardLifecycleTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Demo_CreatesRegistersAndUpgrades_SamePanAndBin()
    {
        var response = await _client.PostAsJsonAsync("/api/demo/e2e", new EndToEndDemoRequest("MCG", "MWE"));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        var demo = JsonSerializer.Deserialize<EndToEndDemoResult>(body, Json);
        Assert.NotNull(demo);
        Assert.Equal("MCG", demo.Registration.SourceProductCode);
        Assert.Equal("MWE", demo.Card.ProductCode);
        Assert.Equal("Active", demo.Upgrade.Status);
        Assert.True(demo.Upgrade.SamePan);
        Assert.True(demo.Upgrade.SameBin);
        Assert.Equal("Local", demo.AlmMode);
        Assert.Equal(demo.Card.Bin, demo.Upgrade.Bin);
        Assert.StartsWith("555555", demo.Card.Bin);
    }

    [Fact]
    public async Task Create_ThenUpgrade_RejectsUnknownProduct()
    {
        var created = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        created.EnsureSuccessStatusCode();
        var card = await created.Content.ReadFromJsonAsync<CardResponse>(Json);
        Assert.NotNull(card);

        var upgrade = await _client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new UpgradeCardRequest("ZZZ"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, upgrade.StatusCode);
    }

    [Fact]
    public async Task Upgrade_ThenRollback_RestoresSourceProduct()
    {
        var created = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        var card = await created.Content.ReadFromJsonAsync<CardResponse>(Json);
        Assert.NotNull(card);

        var upgrade = await _client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new UpgradeCardRequest("MWE", "TEST_UPGRADE"));
        var upgradeBody = await upgrade.Content.ReadAsStringAsync();
        Assert.True(upgrade.IsSuccessStatusCode, upgradeBody);
        var migration = JsonSerializer.Deserialize<MigrationResponse>(upgradeBody, Json);
        Assert.NotNull(migration);
        Assert.Equal("MWE", migration.TargetProductCode);

        var rollback = await _client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades/{migration.MigrationId}/rollback",
            new { });
        var rollbackBody = await rollback.Content.ReadAsStringAsync();
        Assert.True(rollback.IsSuccessStatusCode, rollbackBody);

        var latest = await _client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", Json);
        Assert.Equal("MCG", latest!.ProductCode);
        Assert.Equal(card.MaskedPan, latest.MaskedPan);
        Assert.Equal(card.Bin, latest.Bin);
    }
}
