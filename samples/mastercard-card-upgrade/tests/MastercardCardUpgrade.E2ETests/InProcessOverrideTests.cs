using System.Net;
using System.Net.Http.Json;
using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models.Cards;
using Xunit;

namespace MastercardCardUpgrade.E2ETests;

/// <summary>
/// These need WebApplicationFactory setting overrides, so they only run in-process
/// (local machine). Against a running API they are skipped.
/// </summary>
[Trait("Suite", "E2E")]
[Trait("Host", "InProcess")]
public sealed class InProcessOverrideTests
{
    private static void SkipIfExternal()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(E2EHost.BaseUrlEnv)))
            return;
    }

    [Fact]
    public async Task KillSwitch_RegisterReturns503()
    {
        SkipIfExternal();

        using var factory = E2EHost.IsolatedFactory(("Mastercard:WritesEnabled", "false"));
        var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/cards", new
        {
            productCode = MastercardTestData.SourceProductCode,
            pan = E2EHost.UniquePan(),
            lookupBin = false
        });
        var card = await E2EHost.ReadOk<CardResponse>(created);
        var register = await client.PostAsync($"/api/cards/{card.CardId}/register", null);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, register.StatusCode);
    }

    [Fact]
    public async Task UpgradeTimeout_LeavesProductUnchanged()
    {
        SkipIfExternal();

        using var factory = E2EHost.IsolatedFactory(("Mastercard:SimulateAmbiguousOperation", "update"));
        var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/cards", new
        {
            productCode = MastercardTestData.SourceProductCode,
            pan = E2EHost.UniquePan(),
            lookupBin = false
        });
        var card = await E2EHost.ReadOk<CardResponse>(created);
        (await client.PostAsync($"/api/cards/{card.CardId}/register", null)).EnsureSuccessStatusCode();

        var upgrade = await client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new { targetProductCode = MastercardTestData.TargetProductCode, reason = "TEST_TIMEOUT" });
        var migration = await E2EHost.ReadOk<MigrationResponse>(upgrade);
        Assert.Equal("Unknown", migration.Status);

        var latest = await client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json);
        Assert.Equal(MastercardTestData.SourceProductCode, latest!.ProductCode);

        var treatment = await client.GetFromJsonAsync<TreatmentCheckResponse>(
            $"/api/cards/{card.CardId}/treatment", E2EHost.Json);
        Assert.Equal("UNVERIFIED", treatment!.Outcome);

        var reconciled = await E2EHost.ReadOk<MigrationResponse>(
            await client.PostAsync($"/api/cards/{card.CardId}/upgrades/{migration.MigrationId}/reconcile", null));
        Assert.Equal("ManualReview", reconciled.Status);
    }

    [Fact]
    public async Task DisallowedAccountRange_Returns422()
    {
        SkipIfExternal();

        using var factory = E2EHost.IsolatedFactory(("ProductCatalog:AllowedAccountRangePrefixes:0", "999999"));
        var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/cards", new
        {
            productCode = MastercardTestData.SourceProductCode,
            pan = MastercardTestData.Pan,
            lookupBin = false
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, created.StatusCode);
    }

    [Fact]
    public async Task Demo_PostmanPan_IsolatedHost()
    {
        SkipIfExternal();

        using var factory = E2EHost.IsolatedFactory();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/demo/e2e", new EndToEndDemoRequest(
            MastercardTestData.SourceProductCode,
            MastercardTestData.TargetProductCode,
            MastercardTestData.Pan));
        var demo = await E2EHost.ReadOk<EndToEndDemoResult>(response);
        Assert.Equal(MastercardTestData.MaskedPan, demo.Card.MaskedPan);
        Assert.Equal(MastercardTestData.TargetProductCode, demo.Card.ProductCode);
        Assert.Equal("MATCH", demo.Treatment.Outcome);
    }
}
