using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Options;
using MastercardCardUpgrade.Api.Services;
using Xunit;

namespace MastercardCardUpgrade.UnitTests;

public sealed class AtoZWorkflowTests
{
    [Fact]
    public async Task A_CreateCard_WithPostmanPanAndExpiry()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();

        Assert.Equal(MastercardTestData.MaskedPan, card.MaskedPan);
        Assert.Equal(MastercardTestData.SourceProductCode, card.ProductCode);
        Assert.Equal("Issued", card.Status);
        Assert.StartsWith("555555", card.Bin);
        Assert.Equal(MastercardTestData.Pan[..8], card.Bin);
        Assert.False(h.BinLookup.Called);
    }

    [Fact]
    public async Task B_CreateCard_EnrichesFromBinLookupWhenCredentialsPresent()
    {
        var h = WorkflowHarness.Create(o =>
        {
            o.AuthMode = "Bearer";
            o.Token = "postman-sandbox-token";
        });

        var card = await h.CreatePostmanCardAsync(lookupBin: true);

        Assert.True(h.BinLookup.Called);
        Assert.Equal(MastercardTestData.Pan, h.BinLookup.LastQuery);
        Assert.Equal("00000023460", card.Ica);
        Assert.Equal("MASTERCARD CORPORATE", card.ProductDescription);
        Assert.Equal(MastercardTestData.SourceProductCode, card.ProductCode);
    }

    [Fact]
    public async Task C_GetAndList_ReturnMaskedPanNotClearPan()
    {
        var h = WorkflowHarness.Create();
        var created = await h.CreatePostmanCardAsync();

        var got = h.Lifecycle.Get(created.CardId);
        var list = h.Lifecycle.List();

        Assert.Equal(created.CardId, got.CardId);
        Assert.Equal(MastercardTestData.MaskedPan, got.MaskedPan);
        Assert.Contains(list, c => c.CardId == created.CardId);
        Assert.DoesNotContain(MastercardTestData.Pan, got.MaskedPan);
        Assert.Throws<KeyNotFoundException>(() => h.Lifecycle.Get("card_missing"));
    }

    [Fact]
    public async Task D_Register_UsesAcsAndSetsRegistered()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();

        var registration = await h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId);

        Assert.Equal("Active", registration.Status);
        Assert.Equal(MastercardTestData.RequestId, registration.CorrelationId);
        Assert.Equal("Local", registration.AlmMode);
        Assert.True(registration.SamePan);
        Assert.True(registration.SameBin);
        Assert.Equal("Registered", h.Lifecycle.Get(card.CardId).Status);
        Assert.Equal(MastercardTestData.SourceProductCode, h.Lifecycle.Get(card.CardId).NetworkProductCode);
    }

    [Fact]
    public async Task E_Register_IsIdempotentForSameRequestId()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();

        var first = await h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId);
        var second = await h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId);

        Assert.Equal(first.MigrationId, second.MigrationId);
        Assert.Equal(MastercardTestData.RequestId, second.MastercardRequestId);

        await Assert.ThrowsAsync<IdempotencyConflictException>(() =>
            h.Lifecycle.UpgradeAsync(
                card.CardId,
                new UpgradeCardRequest(MastercardTestData.TargetProductCode, "CUSTOMER_UPGRADE", MastercardTestData.RequestId)));
    }

    [Fact]
    public async Task F_Upgrade_McgToMcw_SwaggerProduct()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId);

        var upgrade = await h.Lifecycle.UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(MastercardTestData.SwaggerProductCode, "CUSTOMER_UPGRADE", MastercardTestData.RequestId + "-mcw"));

        Assert.Equal("Active", upgrade.Status);
        Assert.Equal(MastercardTestData.SourceProductCode, upgrade.SourceProductCode);
        Assert.Equal(MastercardTestData.SwaggerProductCode, upgrade.TargetProductCode);
        Assert.Equal(MastercardTestData.SwaggerProductCode, h.Lifecycle.Get(card.CardId).ProductCode);
        Assert.Equal(MastercardTestData.MaskedPan, upgrade.MaskedPan);
        Assert.True(upgrade.SamePan);
        Assert.True(upgrade.SameBin);
    }

    [Fact]
    public async Task G_Upgrade_McwToMwe_SamePanAndBin()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, null);
        await h.Lifecycle.UpgradeAsync(card.CardId, new UpgradeCardRequest(MastercardTestData.SwaggerProductCode));

        var upgrade = await h.Lifecycle.UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(MastercardTestData.TargetProductCode, "CUSTOMER_UPGRADE"));

        var latest = h.Lifecycle.Get(card.CardId);
        Assert.Equal("Active", upgrade.Status);
        Assert.Equal(MastercardTestData.TargetProductCode, latest.ProductCode);
        Assert.Equal(MastercardTestData.MaskedPan, latest.MaskedPan);
        Assert.Equal(card.Bin, latest.Bin);
        Assert.Equal("MATCH", h.Lifecycle.CheckTreatment(card.CardId).Outcome);
    }

    [Fact]
    public async Task H_Upgrade_RejectsUnknownAlreadyTargetAndDisallowed()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();

        await Assert.ThrowsAsync<EligibilityException>(() =>
            h.Lifecycle.UpgradeAsync(card.CardId, new UpgradeCardRequest("ZZZ")));

        await Assert.ThrowsAsync<EligibilityException>(() =>
            h.Lifecycle.UpgradeAsync(card.CardId, new UpgradeCardRequest(MastercardTestData.SourceProductCode)));

        await h.Lifecycle.CreateAsync(new CreateCardRequest(
            MastercardTestData.BinLookupProductCode,
            PanRules.GenerateMastercardTestPan(),
            MastercardTestData.ExpiryMmYy,
            LookupBin: false));
        var mco = h.Lifecycle.List().Single(c => c.ProductCode == MastercardTestData.BinLookupProductCode);
        await Assert.ThrowsAsync<EligibilityException>(() =>
            h.Lifecycle.UpgradeAsync(mco.CardId, new UpgradeCardRequest(MastercardTestData.TargetProductCode)));
    }

    [Fact]
    public async Task I_UpgradeTimeout_LeavesProductUnchanged_Unknown()
    {
        var h = WorkflowHarness.Create(o => o.SimulateAmbiguousOperation = "update");
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, null);

        var upgrade = await h.Lifecycle.UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(MastercardTestData.TargetProductCode, "TEST_TIMEOUT"));

        Assert.Equal("Unknown", upgrade.Status);
        Assert.Equal(MastercardTestData.SourceProductCode, h.Lifecycle.Get(card.CardId).ProductCode);
        Assert.Equal("UNVERIFIED", h.Lifecycle.CheckTreatment(card.CardId).Outcome);
    }

    [Fact]
    public async Task J_Reconcile_AfterUnknown_GoesToManualReviewWhenAcsHasNoRequest()
    {
        var h = WorkflowHarness.Create(o => o.SimulateAmbiguousOperation = "update");
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, null);
        var upgrade = await h.Lifecycle.UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(MastercardTestData.TargetProductCode));

        var reconciled = await h.Lifecycle.ReconcileAsync(card.CardId, upgrade.MigrationId);

        Assert.Equal("ManualReview", reconciled.Status);
        Assert.Equal(MastercardTestData.SourceProductCode, h.Lifecycle.Get(card.CardId).ProductCode);
        Assert.Contains("correlation_id", reconciled.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task K_Treatment_MatchUnverifiedAndMismatch()
    {
        var matchHarness = WorkflowHarness.Create();
        var demo = await matchHarness.Lifecycle.RunDemoAsync(new EndToEndDemoRequest(
            MastercardTestData.SourceProductCode,
            MastercardTestData.TargetProductCode,
            MastercardTestData.Pan));
        Assert.Equal("MATCH", demo.Treatment.Outcome);

        var unverified = WorkflowHarness.Create();
        var issued = await unverified.CreatePostmanCardAsync();
        Assert.Equal("UNVERIFIED", unverified.Lifecycle.CheckTreatment(issued.CardId).Outcome);

        var mismatch = WorkflowHarness.Create();
        var card = await mismatch.CreatePostmanCardAsync();
        await mismatch.Lifecycle.RegisterAsync(card.CardId, null);
        var stored = mismatch.Store.GetRequired(card.CardId);
        stored.NetworkProductCode = MastercardTestData.SwaggerProductCode;
        mismatch.Store.Update(stored);
        Assert.Equal("MISMATCH", mismatch.Lifecycle.CheckTreatment(card.CardId).Outcome);
    }

    [Fact]
    public async Task L_Rollback_RestoresSourceProduct()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, null);
        var upgrade = await h.Lifecycle.UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(MastercardTestData.TargetProductCode));

        var rolled = await h.Lifecycle.RollbackAsync(card.CardId, upgrade.MigrationId);
        var latest = h.Lifecycle.Get(card.CardId);

        Assert.Equal("Active", rolled.Status);
        Assert.Equal(MastercardTestData.SourceProductCode, latest.ProductCode);
        Assert.Equal(MastercardTestData.MaskedPan, latest.MaskedPan);
        Assert.Equal(card.Bin, latest.Bin);
    }

    [Fact]
    public async Task M_Close_DeletesRegistration()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId);

        var closed = await h.Lifecycle.CloseAsync(card.CardId, MastercardTestData.RequestId + "-close");

        Assert.Equal("Closed", h.Lifecycle.Get(card.CardId).Status);
        Assert.Null(h.Lifecycle.Get(card.CardId).AcsProductRuleId);
        Assert.True(closed.Status is "Active" or "Submitted");
        await Assert.ThrowsAsync<EligibilityException>(() =>
            h.Lifecycle.UpgradeAsync(card.CardId, new UpgradeCardRequest(MastercardTestData.TargetProductCode)));
    }

    [Fact]
    public async Task N_KillSwitch_BlocksWrites()
    {
        var h = WorkflowHarness.Create(o => o.WritesEnabled = false);
        var card = await h.CreatePostmanCardAsync();

        await Assert.ThrowsAsync<KillSwitchException>(() =>
            h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId));
    }

    [Fact]
    public async Task O_Demo_E2E_PostmanData()
    {
        var h = WorkflowHarness.Create();
        var demo = await h.Lifecycle.RunDemoAsync(new EndToEndDemoRequest());

        Assert.Equal(MastercardTestData.MaskedPan, demo.Card.MaskedPan);
        Assert.Equal(MastercardTestData.TargetProductCode, demo.Card.ProductCode);
        Assert.Equal(MastercardTestData.SourceProductCode, demo.Registration.SourceProductCode);
        Assert.Equal("Active", demo.Upgrade.Status);
        Assert.Equal("MATCH", demo.Treatment.Outcome);
        Assert.Equal("Local", demo.AlmMode);
        Assert.True(demo.Upgrade.SamePan);
        Assert.True(demo.Upgrade.SameBin);
    }

    [Fact]
    public async Task P_LegacyUpgrade_ServiceCodeIsAlmNotProduct()
    {
        var h = WorkflowHarness.Create();
        var result = await h.Upgrade.UpgradeAsync(new CardUpgradeRequest(
            MastercardTestData.Pan,
            MastercardTestData.TargetProductCode,
            DateOnly.Parse(MastercardTestData.EffectiveDate),
            MastercardTestData.AlmServiceCode,
            MastercardTestData.RequestId,
            MastercardTestData.SourceProductCode));

        Assert.Equal(MastercardTestData.MaskedPan, result.MaskedPan);
        Assert.Equal(MastercardTestData.TargetProductCode, result.TargetProductCode);
        Assert.Equal("Active", result.SubmissionStatus);
        Assert.Equal(MastercardTestData.TargetProductCode, h.Store.FindByPan(MastercardTestData.Pan)!.ProductCode);
    }

    [Fact]
    public void Q_Catalog_AllowsGraduationPairs_McoHasNoUpgradePath()
    {
        var h = WorkflowHarness.Create();
        Assert.True(h.Catalog.IsAllowedTransition("MCG", "MCW"));
        Assert.True(h.Catalog.IsAllowedTransition("MCG", "MWE"));
        Assert.True(h.Catalog.IsAllowedTransition("MCW", "MWE"));
        Assert.False(h.Catalog.IsAllowedTransition("MCO", "MWE"));
        Assert.Equal("Mastercard Corporate", h.Catalog.GetRequired(MastercardTestData.BinLookupProductCode).Name);
        Assert.Throws<EligibilityException>(() => h.Catalog.GetRequired("ZZZ"));
    }

    [Fact]
    public void R_PanMaskAndRedact_UsePostmanPan()
    {
        Assert.Equal(MastercardTestData.MaskedPan, PanRules.Mask(MastercardTestData.Pan));
        Assert.Equal(MastercardTestData.Pan[..8], PanRules.Bin(MastercardTestData.Pan));
        PanRules.Validate(MastercardTestData.Pan);
        Assert.Throws<EligibilityException>(() => PanRules.Validate("12345"));

        var generated = PanRules.GenerateMastercardTestPan();
        Assert.StartsWith("555555", generated);
        Assert.InRange(generated.Length, 13, 19);
        PanRules.Validate(generated);

        var redacted = PanRedactor.Redact($"PAN={MastercardTestData.Pan} alm={MastercardTestData.AlmServiceCode}");
        Assert.DoesNotContain(MastercardTestData.Pan, redacted);
        Assert.Contains(MastercardTestData.MaskedPan, redacted!);
        Assert.Contains(MastercardTestData.AlmServiceCode, redacted);
    }

    [Fact]
    public async Task S_InvalidCorrelationId_Rejected()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();

        await Assert.ThrowsAsync<EligibilityException>(() =>
            h.Lifecycle.RegisterAsync(card.CardId, "not a valid id"));
    }

    [Fact]
    public async Task T_DuplicatePan_Rejected()
    {
        var h = WorkflowHarness.Create();
        await h.CreatePostmanCardAsync();

        await Assert.ThrowsAsync<EligibilityException>(() => h.CreatePostmanCardAsync());
    }

    [Fact]
    public async Task U_RegisterCannotRollback()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();
        var registration = await h.Lifecycle.RegisterAsync(card.CardId, MastercardTestData.RequestId);

        await Assert.ThrowsAsync<EligibilityException>(() =>
            h.Lifecycle.RollbackAsync(card.CardId, registration.MigrationId));
    }

    [Fact]
    public async Task V_FileStore_SurvivesReload()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mc-cards-{Guid.NewGuid():N}.json");
        try
        {
            var first = WorkflowHarness.Create(o => o.CardStorePath = path);
            var created = await first.CreatePostmanCardAsync();

            var second = WorkflowHarness.Create(o => o.CardStorePath = path);
            var reloaded = second.Lifecycle.Get(created.CardId);

            Assert.Equal(MastercardTestData.MaskedPan, reloaded.MaskedPan);
            Assert.Equal(MastercardTestData.SourceProductCode, reloaded.ProductCode);
            Assert.Equal(MastercardTestData.Pan, second.Store.FindByPan(MastercardTestData.Pan)?.Pan);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task W_ReconcileOpen_ProcessesUnknownMigrations()
    {
        var h = WorkflowHarness.Create(o => o.SimulateAmbiguousOperation = "update");
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, null);
        await h.Lifecycle.UpgradeAsync(card.CardId, new UpgradeCardRequest(MastercardTestData.TargetProductCode));

        var count = await h.Lifecycle.ReconcileOpenAsync();

        Assert.True(count >= 1);
        var open = h.Store.ListNeedingReconcile();
        Assert.DoesNotContain(open, m => m.Status == MigrationStatus.Unknown);
    }

    [Fact]
    public async Task X_Close_RejectsAlreadyClosed()
    {
        var h = WorkflowHarness.Create();
        var card = await h.CreatePostmanCardAsync();
        await h.Lifecycle.RegisterAsync(card.CardId, null);
        await h.Lifecycle.CloseAsync(card.CardId, null);

        await Assert.ThrowsAsync<EligibilityException>(() => h.Lifecycle.CloseAsync(card.CardId, null));
    }

    [Fact]
    public async Task Y_AccountRangeAllowList_RejectsPostmanPanWhenPrefixDoesNotMatch()
    {
        var h = WorkflowHarness.Create(configureCatalog: catalog =>
        {
            catalog.AllowedAccountRangePrefixes = ["999999"];
        });

        await Assert.ThrowsAsync<EligibilityException>(() => h.CreatePostmanCardAsync());

        var allowed = WorkflowHarness.Create(configureCatalog: catalog =>
        {
            catalog.AllowedAccountRangePrefixes = ["555555"];
        });
        var card = await allowed.CreatePostmanCardAsync();
        Assert.Equal(MastercardTestData.MaskedPan, card.MaskedPan);
    }

    [Fact]
    public void Z_Options_BuildMastercardUrlsAndLiveReadinessFromTestData()
    {
        var options = new MastercardOptions
        {
            BaseUrl = "https://sandbox.api.mastercard.com",
            AlmMode = "Mastercard",
            AuthMode = "Bearer",
            Token = "x",
            EncryptionCertificatePath = "",
            DecryptionKeyPath = ""
        };

        Assert.Equal(
            "https://sandbox.api.mastercard.com/bin-ranges/account-searches",
            options.Url(options.Paths.BinLookup).ToString().TrimEnd('/'));
        Assert.Contains("/asc/acs-api/account-registrations", options.Url(options.Paths.AcsRegistrations).ToString());
        Assert.Contains("delete-registrations", options.Url(options.Paths.AcsDeleteRegistrations).ToString());
        Assert.False(options.LiveAcsReady);
        Assert.Equal(MastercardTestData.AccountRange, new MastercardOptions().SandboxSampleAccountRange);
        Assert.Equal(MastercardTestData.Pan, new MastercardOptions().SandboxSamplePan);
        Assert.Equal(MastercardTestData.AlmServiceCode, new MastercardOptions().AlmServiceCode);
        Assert.Equal(MastercardTestData.RequestId, MastercardTestData.RequestId);
        Assert.Equal("PRIMARY_ACCOUNT_NUMBER", MastercardTestData.AccountIndicator);
    }
}
