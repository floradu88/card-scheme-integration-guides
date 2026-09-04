using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models.Cards;
using Microsoft.AspNetCore.Hosting;
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
        var response = await _client.PostAsJsonAsync(
            "/api/demo/e2e",
            new EndToEndDemoRequest(MastercardTestData.SourceProductCode, MastercardTestData.TargetProductCode, MastercardTestData.Pan));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);

        var demo = JsonSerializer.Deserialize<EndToEndDemoResult>(body, Json);
        Assert.NotNull(demo);
        Assert.Equal(MastercardTestData.SourceProductCode, demo.Registration.SourceProductCode);
        Assert.Equal(MastercardTestData.TargetProductCode, demo.Card.ProductCode);
        Assert.Equal("Active", demo.Upgrade.Status);
        Assert.True(demo.Upgrade.SamePan);
        Assert.True(demo.Upgrade.SameBin);
        Assert.Equal("Local", demo.AlmMode);
        Assert.Equal(demo.Card.Bin, demo.Upgrade.Bin);
        Assert.Equal(MastercardTestData.MaskedPan, demo.Card.MaskedPan);
        Assert.StartsWith("555555", demo.Card.Bin);
        Assert.Equal("MATCH", demo.Treatment.Outcome);
        Assert.Equal(MastercardTestData.TargetProductCode, demo.Treatment.NetworkProductCode);
        Assert.DoesNotMatch(@"\d{13,19}", body);
    }

    [Fact]
    public async Task MastercardUpgrade_UsesPostmanCollectionPayload()
    {
        using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/mastercard/upgrade", new
        {
            pan = MastercardTestData.Pan,
            sourceProductCode = MastercardTestData.SourceProductCode,
            targetProductCode = MastercardTestData.TargetProductCode,
            effectiveDate = MastercardTestData.EffectiveDate,
            serviceCode = MastercardTestData.AlmServiceCode,
            correlationId = MastercardTestData.RequestId
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(MastercardTestData.MaskedPan, doc.RootElement.GetProperty("maskedPan").GetString());
        Assert.Equal(MastercardTestData.TargetProductCode, doc.RootElement.GetProperty("targetProductCode").GetString());
        Assert.Equal("Active", doc.RootElement.GetProperty("submissionStatus").GetString());
    }

    [Fact]
    public async Task Register_IsIdempotent_ForSameCorrelationId()
    {
        var created = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        var card = await created.Content.ReadFromJsonAsync<CardResponse>(Json);
        Assert.NotNull(card);

        var correlationId = Guid.NewGuid().ToString();
        var first = await _client.PostAsync($"/api/cards/{card.CardId}/register?correlationId={correlationId}", null);
        var firstBody = await first.Content.ReadAsStringAsync();
        Assert.True(first.IsSuccessStatusCode, firstBody);
        var firstMigration = JsonSerializer.Deserialize<MigrationResponse>(firstBody, Json);

        var second = await _client.PostAsync($"/api/cards/{card.CardId}/register?correlationId={correlationId}", null);
        var secondBody = await second.Content.ReadAsStringAsync();
        Assert.True(second.IsSuccessStatusCode, secondBody);
        var secondMigration = JsonSerializer.Deserialize<MigrationResponse>(secondBody, Json);

        Assert.Equal(firstMigration!.MigrationId, secondMigration!.MigrationId);
    }

    [Fact]
    public async Task UpgradeTimeout_LeavesProductUnchanged_AndTreatmentUnverified()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Mastercard:SimulateAmbiguousOperation", "update");
        });
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        var card = await created.Content.ReadFromJsonAsync<CardResponse>(Json);
        Assert.NotNull(card);

        var registered = await client.PostAsync($"/api/cards/{card.CardId}/register", null);
        registered.EnsureSuccessStatusCode();

        var upgrade = await client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new UpgradeCardRequest("MWE", "TEST_TIMEOUT"));
        var upgradeBody = await upgrade.Content.ReadAsStringAsync();
        Assert.True(upgrade.IsSuccessStatusCode, upgradeBody);
        var migration = JsonSerializer.Deserialize<MigrationResponse>(upgradeBody, Json);
        Assert.Equal("Unknown", migration!.Status);

        var latest = await client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", Json);
        Assert.Equal("MCG", latest!.ProductCode);

        var treatment = await client.GetFromJsonAsync<TreatmentCheckResponse>(
            $"/api/cards/{card.CardId}/treatment", Json);
        Assert.Equal("UNVERIFIED", treatment!.Outcome);
    }

    [Fact]
    public async Task Close_DeletesRegistration()
    {
        var created = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        var card = await created.Content.ReadFromJsonAsync<CardResponse>(Json);
        Assert.NotNull(card);

        (await _client.PostAsync($"/api/cards/{card.CardId}/register", null)).EnsureSuccessStatusCode();
        var closed = await _client.PostAsync($"/api/cards/{card.CardId}/close", null);
        var body = await closed.Content.ReadAsStringAsync();
        Assert.True(closed.IsSuccessStatusCode, body);

        var latest = await _client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", Json);
        Assert.Equal("Closed", latest!.Status);
    }

    [Fact]
    public async Task WritesDisabled_Returns503()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Mastercard:WritesEnabled", "false");
        });
        var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        var card = await created.Content.ReadFromJsonAsync<CardResponse>(Json);

        var register = await client.PostAsync($"/api/cards/{card!.CardId}/register", null);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, register.StatusCode);
    }

    [Fact]
    public async Task DisallowedAccountRange_Returns422()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ProductCatalog:AllowedAccountRangePrefixes:0", "999999");
        });
        var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/cards", new CreateCardRequest("MCG", LookupBin: false));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, created.StatusCode);
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
