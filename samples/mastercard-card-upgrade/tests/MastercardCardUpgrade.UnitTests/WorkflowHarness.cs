using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models;
using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Options;
using MastercardCardUpgrade.Api.Services;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.UnitTests;

internal sealed class FakeBinLookupClient : IMastercardBinLookupClient
{
    public bool Called { get; private set; }
    public string? LastQuery { get; private set; }

    public Task<BinAccountRangeResponse> SearchAccountRangeAsync(
        string panOrAccountRange,
        CancellationToken cancellationToken = default)
    {
        Called = true;
        LastQuery = panOrAccountRange;
        return Task.FromResult(new BinAccountRangeResponse
        {
            LowAccountRange = 2229293150000000000,
            HighAccountRange = 2229293200000000000,
            AcceptanceBrand = "MCC",
            Ica = "00000023460",
            ProductCode = MastercardTestData.BinLookupProductCode,
            ProductDescription = "MASTERCARD CORPORATE",
            FundingSource = "DEBIT",
            ConsumerType = "CONSUMER"
        });
    }
}

internal sealed class WorkflowHarness
{
    public MastercardOptions Options { get; }
    public ProductCatalogOptions CatalogOptions { get; }
    public ICardStore Store { get; }
    public LocalAcsClient Acs { get; }
    public FakeBinLookupClient BinLookup { get; }
    public CardLifecycleService Lifecycle { get; }
    public MastercardUpgradeService Upgrade { get; }
    public ProductCatalog Catalog { get; }
    public EligibilityService Eligibility { get; }

    private WorkflowHarness(
        MastercardOptions options,
        ProductCatalogOptions catalogOptions,
        ICardStore store,
        LocalAcsClient acs,
        FakeBinLookupClient binLookup,
        CardLifecycleService lifecycle,
        MastercardUpgradeService upgrade,
        ProductCatalog catalog,
        EligibilityService eligibility)
    {
        Options = options;
        CatalogOptions = catalogOptions;
        Store = store;
        Acs = acs;
        BinLookup = binLookup;
        Lifecycle = lifecycle;
        Upgrade = upgrade;
        Catalog = catalog;
        Eligibility = eligibility;
    }

    public static WorkflowHarness Create(Action<MastercardOptions>? configureMastercard = null, Action<ProductCatalogOptions>? configureCatalog = null)
    {
        var mastercard = new MastercardOptions
        {
            BaseUrl = "https://sandbox.api.mastercard.com",
            AuthMode = "OAuth1",
            AlmMode = "Local",
            WritesEnabled = true,
            SandboxSampleAccountRange = MastercardTestData.AccountRange,
            SandboxSamplePan = MastercardTestData.Pan,
            SandboxSampleExpiryMmYy = MastercardTestData.ExpiryMmYy,
            SandboxSourceProductCode = MastercardTestData.SourceProductCode,
            SandboxTargetProductCode = MastercardTestData.TargetProductCode,
            AlmServiceCode = MastercardTestData.AlmServiceCode
        };
        configureMastercard?.Invoke(mastercard);

        var catalogOptions = DefaultCatalog();
        configureCatalog?.Invoke(catalogOptions);

        var mastercardOpts = Microsoft.Extensions.Options.Options.Create(mastercard);
        var catalogOpts = Microsoft.Extensions.Options.Options.Create(catalogOptions);
        var store = new InMemoryCardStore(mastercardOpts);
        var catalog = new ProductCatalog(catalogOpts);
        var eligibility = new EligibilityService(catalog);
        var acs = new LocalAcsClient(mastercardOpts);
        var binLookup = new FakeBinLookupClient();
        var lifecycle = new CardLifecycleService(store, catalog, eligibility, acs, binLookup, mastercardOpts);
        var upgrade = new MastercardUpgradeService(store, lifecycle, mastercardOpts);
        return new WorkflowHarness(mastercard, catalogOptions, store, acs, binLookup, lifecycle, upgrade, catalog, eligibility);
    }

    public static ProductCatalogOptions DefaultCatalog() => new()
    {
        Products =
        [
            new() { Code = "MCG", Name = "Mastercard Gold", LineOfBusiness = "CONSUMER_CREDIT" },
            new() { Code = "MCW", Name = "Mastercard World", LineOfBusiness = "CONSUMER_CREDIT" },
            new() { Code = "MWE", Name = "Mastercard World Elite", LineOfBusiness = "CONSUMER_CREDIT" },
            new() { Code = "MCO", Name = "Mastercard Corporate", LineOfBusiness = "COMMERCIAL" }
        ],
        AllowedTransitions =
        [
            new() { From = "MCG", To = ["MCW", "MWE"] },
            new() { From = "MCW", To = ["MWE", "MCG"] },
            new() { From = "MWE", To = ["MCW", "MCG"] }
        ]
    };

    public Task<CardResponse> CreatePostmanCardAsync(string productCode = MastercardTestData.SourceProductCode, bool lookupBin = false) =>
        Lifecycle.CreateAsync(new CreateCardRequest(
            productCode,
            MastercardTestData.Pan,
            MastercardTestData.ExpiryMmYy,
            lookupBin));
}
