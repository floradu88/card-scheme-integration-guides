using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Models.Cards;
using Xunit;

namespace MastercardCardUpgrade.E2ETests;

[Collection("E2E")]
[Trait("Suite", "E2E")]
public sealed class AtoZHttpWorkflowTests
{
    private readonly E2EHost _host;

    public AtoZHttpWorkflowTests(E2EHost host) => _host = host;

    [Fact]
    public async Task A_Status_Swagger_And_TestData()
    {
        Assert.Equal("https://sandbox.api.mastercard.com", _host.Status.BaseUrl);
        Assert.Contains("/bin-ranges/account-searches", _host.Status.BinLookupUrl);
        Assert.Contains("/asc/acs-api/account-registrations", _host.Status.AcsRegistrationsUrl);
        Assert.NotNull(_host.Status.TestData);
        Assert.Equal(MastercardTestData.AccountRange, _host.Status.TestData!.AccountRange);
        Assert.Equal(MastercardTestData.Pan, _host.Status.TestData.Pan);
        Assert.Equal(MastercardTestData.AlmServiceCode, _host.Status.TestData.AlmServiceCode);

        var swagger = await _host.Client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
        var swaggerJson = await swagger.Content.ReadAsStringAsync();
        Assert.Contains("/api/cards", swaggerJson);
        Assert.Contains("/api/demo/e2e", swaggerJson);
    }

    [Fact]
    public async Task B_Products_IncludeGraduationAndBinLookupCodes()
    {
        var products = await _host.Client.GetFromJsonAsync<JsonElement>("/api/products");
        var codes = products.EnumerateArray().Select(p => p.GetProperty("code").GetString()).ToHashSet();
        Assert.Contains(MastercardTestData.SourceProductCode, codes);
        Assert.Contains(MastercardTestData.SwaggerProductCode, codes);
        Assert.Contains(MastercardTestData.TargetProductCode, codes);
        Assert.Contains(MastercardTestData.BinLookupProductCode, codes);
    }

    [Fact]
    public async Task C_Create_Get_List_UseUniquePan()
    {
        var pan = E2EHost.UniquePan();
        var card = await _host.CreateCardAsync(pan);

        Assert.Equal(MastercardTestData.SourceProductCode, card.ProductCode);
        Assert.Equal("Issued", card.Status);
        Assert.StartsWith("555555", card.Bin);
        Assert.DoesNotContain(pan, card.MaskedPan);

        var got = await _host.Client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json);
        Assert.Equal(card.CardId, got!.CardId);

        var list = await _host.Client.GetFromJsonAsync<List<CardResponse>>("/api/cards", E2EHost.Json);
        Assert.Contains(list!, c => c.CardId == card.CardId);

        var missing = await _host.Client.GetAsync("/api/cards/card_does_not_exist");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task D_Register_Then_ListMigrations()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var correlationId = E2EHost.UniqueRequestId();
        var registration = await _host.RegisterAsync(card.CardId, correlationId);

        Assert.Equal("Active", registration.Status);
        Assert.Equal(correlationId, registration.CorrelationId);
        Assert.True(registration.SamePan);
        Assert.True(registration.SameBin);

        var latest = await _host.Client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json);
        Assert.Equal("Registered", latest!.Status);

        var migrations = await _host.Client.GetFromJsonAsync<List<MigrationResponse>>(
            $"/api/cards/{card.CardId}/upgrades", E2EHost.Json);
        Assert.Contains(migrations!, m => m.MigrationId == registration.MigrationId);
    }

    [Fact]
    public async Task E_Register_IsIdempotent_ForSameCorrelationId()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var correlationId = E2EHost.UniqueRequestId();
        var first = await _host.RegisterAsync(card.CardId, correlationId);
        var second = await _host.RegisterAsync(card.CardId, correlationId);
        Assert.Equal(first.MigrationId, second.MigrationId);
    }

    [Fact]
    public async Task F_Upgrade_McgToMcw_SwaggerProduct()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        await _host.RegisterAsync(card.CardId);
        var upgrade = await _host.UpgradeAsync(card.CardId, MastercardTestData.SwaggerProductCode, "CUSTOMER_UPGRADE");

        Assert.Equal("Active", upgrade.Status);
        Assert.Equal(MastercardTestData.SwaggerProductCode, upgrade.TargetProductCode);
        Assert.True(upgrade.SamePan);
        Assert.True(upgrade.SameBin);

        var latest = await _host.Client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json);
        Assert.Equal(MastercardTestData.SwaggerProductCode, latest!.ProductCode);
    }

    [Fact]
    public async Task G_Upgrade_McwToMwe_ThenTreatmentMatch()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        await _host.RegisterAsync(card.CardId);
        await _host.UpgradeAsync(card.CardId, MastercardTestData.SwaggerProductCode);
        var upgrade = await _host.UpgradeAsync(card.CardId, MastercardTestData.TargetProductCode);

        Assert.Equal("Active", upgrade.Status);
        var treatment = await _host.Client.GetFromJsonAsync<TreatmentCheckResponse>(
            $"/api/cards/{card.CardId}/treatment", E2EHost.Json);
        Assert.Equal("MATCH", treatment!.Outcome);
        Assert.Equal(MastercardTestData.TargetProductCode, treatment.IssuerProductCode);
    }

    [Fact]
    public async Task H_Upgrade_RejectsUnknownProduct()
    {
        var card = await _host.CreateCardAsync();
        var upgrade = await _host.Client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new { targetProductCode = "ZZZ" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, upgrade.StatusCode);
    }

    [Fact]
    public async Task I_Create_RejectsUnknownProduct()
    {
        var response = await _host.Client.PostAsJsonAsync("/api/cards", new
        {
            productCode = "ZZZ",
            pan = E2EHost.UniquePan(),
            lookupBin = false
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task J_Reconcile_AfterUpgrade()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        await _host.RegisterAsync(card.CardId);
        var upgrade = await _host.UpgradeAsync(card.CardId, MastercardTestData.TargetProductCode);

        var reconciled = await E2EHost.ReadOk<MigrationResponse>(
            await _host.Client.PostAsync($"/api/cards/{card.CardId}/upgrades/{upgrade.MigrationId}/reconcile", null));
        Assert.Equal("Active", reconciled.Status);

        var batch = await _host.Client.PostAsync("/api/migrations/reconcile", null);
        var batchBody = await E2EHost.ReadOk<JsonElement>(batch);
        Assert.True(batchBody.TryGetProperty("reconciled", out _));
    }

    [Fact]
    public async Task K_AlmStatus_ByCorrelationId()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var registration = await _host.RegisterAsync(card.CardId, E2EHost.UniqueRequestId());

        var status = await _host.Client.GetAsync($"/api/mastercard/alm/status/{registration.MastercardRequestId}");
        Assert.True(status.IsSuccessStatusCode, await status.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task L_Rollback_RestoresSourceProduct()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var upgrade = await _host.UpgradeAsync(card.CardId, MastercardTestData.TargetProductCode, "TEST_UPGRADE");

        var rollback = await E2EHost.ReadOk<MigrationResponse>(
            await _host.Client.PostAsJsonAsync($"/api/cards/{card.CardId}/upgrades/{upgrade.MigrationId}/rollback", new { }));
        Assert.Equal("Active", rollback.Status);

        var latest = await _host.Client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json);
        Assert.Equal(MastercardTestData.SourceProductCode, latest!.ProductCode);
        Assert.Equal(card.MaskedPan, latest.MaskedPan);
        Assert.Equal(card.Bin, latest.Bin);
    }

    [Fact]
    public async Task M_Close_ThenUpgradeFails()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        await _host.RegisterAsync(card.CardId);
        var closed = await E2EHost.ReadOk<MigrationResponse>(
            await _host.Client.PostAsync($"/api/cards/{card.CardId}/close", null));
        Assert.True(closed.Status is "Active" or "Submitted");

        var latest = await _host.Client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json);
        Assert.Equal("Closed", latest!.Status);

        var upgrade = await _host.Client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new { targetProductCode = MastercardTestData.TargetProductCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, upgrade.StatusCode);
    }

    [Fact]
    public async Task N_DuplicatePan_Returns422()
    {
        var pan = E2EHost.UniquePan();
        await _host.CreateCardAsync(pan);
        var second = await _host.Client.PostAsJsonAsync("/api/cards", new
        {
            productCode = MastercardTestData.SourceProductCode,
            pan,
            expiryMmYy = MastercardTestData.ExpiryMmYy,
            lookupBin = false
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
    }

    [Fact]
    public async Task O_Demo_E2E_UniquePan_SamePanAndBin()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var pan = E2EHost.UniquePan();
        var response = await _host.Client.PostAsJsonAsync("/api/demo/e2e", new EndToEndDemoRequest(
            MastercardTestData.SourceProductCode,
            MastercardTestData.TargetProductCode,
            pan));
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        var demo = JsonSerializer.Deserialize<EndToEndDemoResult>(body, E2EHost.Json);
        Assert.NotNull(demo);

        Assert.Equal(MastercardTestData.TargetProductCode, demo!.Card.ProductCode);
        Assert.Equal("Active", demo.Upgrade.Status);
        Assert.True(demo.Upgrade.SamePan);
        Assert.True(demo.Upgrade.SameBin);
        Assert.Equal("MATCH", demo.Treatment.Outcome);
        Assert.DoesNotContain(pan, body);
        Assert.DoesNotMatch(@"\d{13,19}", body);
    }

    [Fact]
    public async Task P_LegacyMastercardUpgrade_PostmanPayloadShape()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var pan = E2EHost.UniquePan();
        var response = await _host.Client.PostAsJsonAsync("/api/mastercard/upgrade", new
        {
            pan,
            sourceProductCode = MastercardTestData.SourceProductCode,
            targetProductCode = MastercardTestData.TargetProductCode,
            effectiveDate = MastercardTestData.EffectiveDate,
            serviceCode = MastercardTestData.AlmServiceCode,
            correlationId = E2EHost.UniqueRequestId()
        });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(MastercardTestData.TargetProductCode, doc.RootElement.GetProperty("targetProductCode").GetString());
        Assert.Equal("Active", doc.RootElement.GetProperty("submissionStatus").GetString());
    }

    [Fact]
    public async Task Q_InvalidCorrelationId_Returns422()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var response = await _host.Client.PostAsync(
            $"/api/cards/{card.CardId}/register?correlationId={Uri.EscapeDataString("not a valid id")}",
            null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task R_AlreadyOnTarget_Returns422()
    {
        var card = await _host.CreateCardAsync();
        var response = await _host.Client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new { targetProductCode = MastercardTestData.SourceProductCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task S_Treatment_Unverified_OnIssuedCard()
    {
        var card = await _host.CreateCardAsync();
        var treatment = await _host.Client.GetFromJsonAsync<TreatmentCheckResponse>(
            $"/api/cards/{card.CardId}/treatment", E2EHost.Json);
        Assert.Equal("UNVERIFIED", treatment!.Outcome);
    }

    [Fact]
    public async Task T_IdempotencyConflict_SameRequestIdDifferentOperation()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var requestId = E2EHost.UniqueRequestId();
        await _host.RegisterAsync(card.CardId, requestId);

        var upgrade = await _host.Client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new
            {
                targetProductCode = MastercardTestData.TargetProductCode,
                correlationId = requestId
            });
        Assert.Equal(HttpStatusCode.Conflict, upgrade.StatusCode);
    }

    [Fact]
    public async Task U_RegisterRollback_Returns422()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        var registration = await _host.RegisterAsync(card.CardId);
        var rollback = await _host.Client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades/{registration.MigrationId}/rollback",
            new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rollback.StatusCode);
    }

    [Fact]
    public async Task V_CloseTwice_Returns422()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync();
        await _host.RegisterAsync(card.CardId);
        (await _host.Client.PostAsync($"/api/cards/{card.CardId}/close", null)).EnsureSuccessStatusCode();
        var second = await _host.Client.PostAsync($"/api/cards/{card.CardId}/close", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
    }

    [Fact]
    public async Task W_Mco_CannotGraduateToMwe()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var card = await _host.CreateCardAsync(productCode: MastercardTestData.BinLookupProductCode);
        var upgrade = await _host.Client.PostAsJsonAsync(
            $"/api/cards/{card.CardId}/upgrades",
            new { targetProductCode = MastercardTestData.TargetProductCode });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, upgrade.StatusCode);
    }

    [Fact]
    public async Task X_GetMissingMigration_Returns404()
    {
        var card = await _host.CreateCardAsync();
        var response = await _host.Client.PostAsync(
            $"/api/cards/{card.CardId}/upgrades/mig_does_not_exist/reconcile",
            null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Y_BinLookup_WithoutCredentials_FailsOnLiveCall()
    {
        if (_host.CredentialsConfigured)
            return;

        var response = await _host.Client.PostAsJsonAsync(
            "/api/mastercard/sandbox/bin-lookup",
            new { panOrAccountRange = MastercardTestData.AccountRange });
        Assert.False(response.IsSuccessStatusCode);
        Assert.InRange((int)response.StatusCode, 400, 599);
    }

    [Fact]
    public async Task Z_FullHappyPath_CreateRegisterUpgradeTreatRollbackClose()
    {
        if (_host.ShouldSkipLiveAcsWrites())
            return;
        var pan = E2EHost.UniquePan();
        var card = await _host.CreateCardAsync(pan);
        var register = await _host.RegisterAsync(card.CardId, E2EHost.UniqueRequestId());
        var toMcw = await _host.UpgradeAsync(card.CardId, MastercardTestData.SwaggerProductCode);
        var toMwe = await _host.UpgradeAsync(card.CardId, MastercardTestData.TargetProductCode);
        var treatment = await _host.Client.GetFromJsonAsync<TreatmentCheckResponse>(
            $"/api/cards/{card.CardId}/treatment", E2EHost.Json);
        var rollback = await E2EHost.ReadOk<MigrationResponse>(
            await _host.Client.PostAsJsonAsync($"/api/cards/{card.CardId}/upgrades/{toMwe.MigrationId}/rollback", new { }));
        var closed = await E2EHost.ReadOk<MigrationResponse>(
            await _host.Client.PostAsync($"/api/cards/{card.CardId}/close", null));

        Assert.Equal("Active", register.Status);
        Assert.Equal("Active", toMcw.Status);
        Assert.Equal("Active", toMwe.Status);
        Assert.Equal("MATCH", treatment!.Outcome);
        Assert.Equal("Active", rollback.Status);
        Assert.Equal("Closed", (await _host.Client.GetFromJsonAsync<CardResponse>($"/api/cards/{card.CardId}", E2EHost.Json))!.Status);
        Assert.True(closed.SamePan);
        Assert.True(toMwe.SameBin);
    }
}
