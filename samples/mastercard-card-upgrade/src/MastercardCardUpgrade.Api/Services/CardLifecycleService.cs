using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Models.Acs;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public interface ICardLifecycleService
{
    Task<CardResponse> CreateAsync(CreateCardRequest request, CancellationToken cancellationToken = default);
    CardResponse Get(string cardId);
    IReadOnlyList<CardResponse> List();
    Task<MigrationResponse> RegisterAsync(string cardId, string? correlationId, CancellationToken cancellationToken = default);
    Task<MigrationResponse> UpgradeAsync(string cardId, UpgradeCardRequest request, CancellationToken cancellationToken = default);
    Task<MigrationResponse> ReconcileAsync(string cardId, string migrationId, CancellationToken cancellationToken = default);
    Task<MigrationResponse> RollbackAsync(string cardId, string migrationId, CancellationToken cancellationToken = default);
    IReadOnlyList<MigrationResponse> ListMigrations(string cardId);
    Task<EndToEndDemoResult> RunDemoAsync(EndToEndDemoRequest request, CancellationToken cancellationToken = default);
}

public sealed class CardLifecycleService : ICardLifecycleService
{
    private readonly ICardStore _store;
    private readonly IProductCatalog _catalog;
    private readonly IEligibilityService _eligibility;
    private readonly IAcsClient _acs;
    private readonly IMastercardBinLookupClient _binLookup;
    private readonly MastercardOptions _options;

    public CardLifecycleService(
        ICardStore store,
        IProductCatalog catalog,
        IEligibilityService eligibility,
        IAcsClient acs,
        IMastercardBinLookupClient binLookup,
        IOptions<MastercardOptions> options)
    {
        _store = store;
        _catalog = catalog;
        _eligibility = eligibility;
        _acs = acs;
        _binLookup = binLookup;
        _options = options.Value;
    }

    public async Task<CardResponse> CreateAsync(CreateCardRequest request, CancellationToken cancellationToken = default)
    {
        var pan = string.IsNullOrWhiteSpace(request.Pan)
            ? PanRules.GenerateMastercardTestPan()
            : PanRules.Normalize(request.Pan);

        _eligibility.ValidateCreate(request.ProductCode, pan);

        if (_store.FindByPan(pan) is not null)
            throw new EligibilityException("A card with this PAN already exists.");

        var product = _catalog.GetRequired(request.ProductCode);
        var card = new CardAccount
        {
            CardId = $"card_{Guid.NewGuid():N}"[..21],
            Pan = pan,
            MaskedPan = PanRules.Mask(pan),
            Bin = PanRules.Bin(pan),
            ProductCode = product.Code,
            ProductDescription = product.Name,
            ExpiryMmYy = request.ExpiryMmYy ?? DateTime.UtcNow.AddYears(3).ToString("MMyy")
        };

        if (request.LookupBin && _options.HasCredentials)
        {
            try
            {
                var bin = await _binLookup.SearchAccountRangeAsync(pan, cancellationToken);
                card.Ica = bin.Ica;
                card.ProductDescription = bin.ProductDescription ?? card.ProductDescription;
            }
            catch (Exception)
            {
                // BIN Lookup is enrichment only for card issuance; ACS registration is the write path.
            }
        }

        _store.Add(card);
        return ToResponse(card);
    }

    public CardResponse Get(string cardId) => ToResponse(_store.GetRequired(cardId));

    public IReadOnlyList<CardResponse> List() => _store.List().Select(ToResponse).ToList();

    public async Task<MigrationResponse> RegisterAsync(
        string cardId,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        var card = _store.GetRequired(cardId);
        if (card.Status == CardStatus.Registered && !string.IsNullOrWhiteSpace(card.AcsProductRuleId))
            throw new EligibilityException("Card is already registered for Product Graduation Plus.");

        var requestId = correlationId ?? Guid.NewGuid().ToString();
        var result = await _acs.RegisterPanAsync(
            card.Pan,
            card.ProductCode,
            card.ExpiryMmYy,
            requestId,
            cancellationToken);

        ApplyAcs(card, result);
        card.Status = result.Accepted ? CardStatus.Registered : card.Status;
        _store.Update(card);

        var migration = _store.AddMigration(new ProductMigration
        {
            MigrationId = $"mig_{Guid.NewGuid():N}"[..20],
            CardId = card.CardId,
            SourceProductCode = card.ProductCode,
            TargetProductCode = card.ProductCode,
            Reason = "ACS_REGISTER",
            CorrelationId = requestId,
            Status = result.Accepted ? MigrationStatus.Active : MigrationStatus.Rejected,
            MastercardRequestId = result.RequestId,
            ProductRuleId = result.ProductRuleId,
            MastercardRawResponse = result.RawResponse,
            FailureReason = result.Accepted ? null : result.Status,
            SamePan = true,
            SameBin = true
        });

        return ToMigration(card, migration);
    }

    public async Task<MigrationResponse> UpgradeAsync(
        string cardId,
        UpgradeCardRequest request,
        CancellationToken cancellationToken = default)
    {
        var card = _store.GetRequired(cardId);
        var source = card.ProductCode;
        var panBefore = card.Pan;
        var binBefore = card.Bin;

        _eligibility.ValidateUpgrade(card, request.TargetProductCode);

        if (card.Status != CardStatus.Registered)
            await RegisterAsync(cardId, null, cancellationToken);

        card = _store.GetRequired(cardId);
        var requestId = request.CorrelationId ?? Guid.NewGuid().ToString();

        var result = await _acs.UpdatePanProductAsync(
            card.Pan,
            request.TargetProductCode,
            card.AcsProductRuleId,
            card.ExpiryMmYy,
            requestId,
            cancellationToken);

        var samePan = card.Pan == panBefore;
        var sameBin = card.Bin == binBefore;
        if (!samePan || !sameBin)
            throw new EligibilityException("PAN/BIN invariant violated.");

        var status = MapStatus(result);
        if (result.Accepted)
        {
            ApplyAcs(card, result);
            if (status is MigrationStatus.Accepted or MigrationStatus.Active)
                card.ProductCode = request.TargetProductCode;
            _store.Update(card);
        }

        var migration = _store.AddMigration(new ProductMigration
        {
            MigrationId = $"mig_{Guid.NewGuid():N}"[..20],
            CardId = card.CardId,
            SourceProductCode = source,
            TargetProductCode = request.TargetProductCode,
            Reason = request.Reason ?? "CUSTOMER_UPGRADE",
            CorrelationId = requestId,
            Status = status,
            MastercardRequestId = result.RequestId,
            ProductRuleId = result.ProductRuleId,
            MastercardRawResponse = result.RawResponse,
            FailureReason = result.Accepted ? null : result.Status,
            SamePan = samePan,
            SameBin = sameBin
        });

        if (status == MigrationStatus.Submitted)
            return await ReconcileAsync(card.CardId, migration.MigrationId, cancellationToken);

        return ToMigration(card, migration);
    }

    public async Task<MigrationResponse> ReconcileAsync(
        string cardId,
        string migrationId,
        CancellationToken cancellationToken = default)
    {
        var card = _store.GetRequired(cardId);
        var migration = _store.GetMigrationRequired(cardId, migrationId);
        var requestId = migration.MastercardRequestId ?? migration.CorrelationId;

        var result = await _acs.GetStatusAsync(requestId, cancellationToken);
        migration.MastercardRawResponse = result.RawResponse;
        migration.ProductRuleId = result.ProductRuleId ?? migration.ProductRuleId;
        migration.Status = MapStatus(result, treatFinalAsActive: true);

        if (result.Accepted && migration.Status == MigrationStatus.Active)
        {
            card.ProductCode = migration.TargetProductCode;
            ApplyAcs(card, result);
            _store.Update(card);
        }

        if (!result.Accepted)
            migration.FailureReason = result.Status;

        _store.UpdateMigration(migration);
        return ToMigration(card, migration);
    }

    public async Task<MigrationResponse> RollbackAsync(
        string cardId,
        string migrationId,
        CancellationToken cancellationToken = default)
    {
        var original = _store.GetMigrationRequired(cardId, migrationId);
        if (original.Reason == "ACS_REGISTER")
            throw new EligibilityException("Register events cannot be rolled back with Product Graduation; close the PAN instead.");

        var rolled = await UpgradeAsync(
            cardId,
            new UpgradeCardRequest(original.SourceProductCode, "ROLLBACK", null),
            cancellationToken);

        original.Status = MigrationStatus.RolledBack;
        _store.UpdateMigration(original);
        return rolled;
    }

    public IReadOnlyList<MigrationResponse> ListMigrations(string cardId)
    {
        var card = _store.GetRequired(cardId);
        return _store.ListMigrations(cardId).Select(m => ToMigration(card, m)).ToList();
    }

    public async Task<EndToEndDemoResult> RunDemoAsync(
        EndToEndDemoRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = string.IsNullOrWhiteSpace(request.SourceProductCode) ? "MCG" : request.SourceProductCode;
        var target = string.IsNullOrWhiteSpace(request.TargetProductCode) ? "MWE" : request.TargetProductCode;

        var card = await CreateAsync(
            new CreateCardRequest(source, request.Pan, LookupBin: false),
            cancellationToken);

        var registration = await RegisterAsync(card.CardId, null, cancellationToken);
        var upgrade = await UpgradeAsync(
            card.CardId,
            new UpgradeCardRequest(target, "DEMO_UPGRADE"),
            cancellationToken);

        var latest = Get(card.CardId);
        return new EndToEndDemoResult(
            latest,
            registration,
            upgrade,
            _acs.Mode,
            $"Created {latest.MaskedPan} as {source}, registered in PGP, upgraded {source} → {latest.ProductCode} with the same PAN/BIN.");
    }

    private static void ApplyAcs(CardAccount card, AcsOperationResult result)
    {
        card.LastAcsRequestId = result.RequestId;
        if (!string.IsNullOrWhiteSpace(result.ProductRuleId))
            card.AcsProductRuleId = result.ProductRuleId;
    }

    private static MigrationStatus MapStatus(AcsOperationResult result, bool treatFinalAsActive = false)
    {
        if (!result.Accepted)
            return MigrationStatus.Rejected;

        if (string.Equals(result.ResponseType, AcsResponseTypes.Final, StringComparison.OrdinalIgnoreCase)
            || treatFinalAsActive && result.Accepted)
            return MigrationStatus.Active;

        return MigrationStatus.Submitted;
    }

    private CardResponse ToResponse(CardAccount card) =>
        new(
            card.CardId,
            card.MaskedPan,
            card.Bin,
            card.ProductCode,
            card.ProductDescription,
            card.Ica,
            card.Status.ToString(),
            card.AcsProductRuleId,
            card.CreatedAt,
            card.UpdatedAt);

    private MigrationResponse ToMigration(CardAccount card, ProductMigration migration) =>
        new(
            migration.MigrationId,
            card.CardId,
            card.MaskedPan,
            card.Bin,
            migration.SourceProductCode,
            migration.TargetProductCode,
            migration.Status.ToString(),
            migration.CorrelationId,
            migration.MastercardRequestId,
            migration.ProductRuleId,
            migration.SamePan,
            migration.SameBin,
            _acs.Mode,
            migration.MastercardRawResponse,
            migration.FailureReason);
}
