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
    Task<int> ReconcileOpenAsync(CancellationToken cancellationToken = default);
    Task<MigrationResponse> RollbackAsync(string cardId, string migrationId, CancellationToken cancellationToken = default);
    Task<MigrationResponse> CloseAsync(string cardId, string? correlationId, CancellationToken cancellationToken = default);
    TreatmentCheckResponse CheckTreatment(string cardId);
    IReadOnlyList<MigrationResponse> ListMigrations(string cardId);
    Task<EndToEndDemoResult> RunDemoAsync(EndToEndDemoRequest request, CancellationToken cancellationToken = default);
}

public sealed class CardLifecycleService : ICardLifecycleService
{
    private static readonly HashSet<MigrationStatus> OpenStatuses =
    [
        MigrationStatus.Submitted,
        MigrationStatus.Unknown,
        MigrationStatus.Reconciling,
        MigrationStatus.Accepted
    ];

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
        EnsureWritesEnabled();
        var card = _store.GetRequired(cardId);
        var requestId = NewRequestId(correlationId);

        if (TryReplay(requestId, "ACS_REGISTER", card.ProductCode, out var replayed))
            return ToMigration(card, replayed);

        if (card.Status == CardStatus.Registered && !string.IsNullOrWhiteSpace(card.AcsProductRuleId))
            throw new EligibilityException("Card is already registered for Product Graduation Plus.");

        var migration = NewMigration(card, card.ProductCode, card.ProductCode, "ACS_REGISTER", requestId);
        try
        {
            var result = await _acs.RegisterPanAsync(
                card.Pan,
                card.ProductCode,
                card.ExpiryMmYy,
                requestId,
                cancellationToken);

            ApplyWriteResult(card, migration, result, updateProduct: false);
            if (result.Accepted)
                card.Status = CardStatus.Registered;
            _store.Update(card);
        }
        catch (AcsAmbiguousOutcomeException ex)
        {
            MarkUnknown(migration, ex);
        }

        _store.AddMigration(migration);
        if (migration.Status == MigrationStatus.Submitted)
            return await ReconcileAsync(card.CardId, migration.MigrationId, cancellationToken);

        return ToMigration(_store.GetRequired(cardId), migration);
    }

    public async Task<MigrationResponse> UpgradeAsync(
        string cardId,
        UpgradeCardRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureWritesEnabled();
        var card = _store.GetRequired(cardId);
        var source = card.ProductCode;
        var panBefore = card.Pan;
        var binBefore = card.Bin;
        var requestId = NewRequestId(request.CorrelationId);

        if (TryReplay(requestId, request.Reason ?? "CUSTOMER_UPGRADE", request.TargetProductCode, out var replayed))
            return ToMigration(card, replayed);

        _eligibility.ValidateUpgrade(card, request.TargetProductCode);

        if (card.Status != CardStatus.Registered)
            await RegisterAsync(cardId, null, cancellationToken);

        card = _store.GetRequired(cardId);
        var migration = NewMigration(card, source, request.TargetProductCode, request.Reason ?? "CUSTOMER_UPGRADE", requestId);

        try
        {
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

            migration.SamePan = samePan;
            migration.SameBin = sameBin;
            ApplyWriteResult(card, migration, result, updateProduct: true);
            _store.Update(card);
        }
        catch (AcsAmbiguousOutcomeException ex)
        {
            MarkUnknown(migration, ex);
        }

        _store.AddMigration(migration);
        if (migration.Status == MigrationStatus.Submitted)
            return await ReconcileAsync(card.CardId, migration.MigrationId, cancellationToken);

        return ToMigration(_store.GetRequired(cardId), migration);
    }

    public async Task<MigrationResponse> ReconcileAsync(
        string cardId,
        string migrationId,
        CancellationToken cancellationToken = default)
    {
        var card = _store.GetRequired(cardId);
        var migration = _store.GetMigrationRequired(cardId, migrationId);
        var requestId = migration.MastercardRequestId ?? migration.CorrelationId;
        migration.Status = MigrationStatus.Reconciling;
        migration.AttemptCount++;
        _store.UpdateMigration(migration);

        try
        {
            var result = await _acs.GetStatusAsync(requestId, cancellationToken);
            ApplyWriteResult(card, migration, result, updateProduct: migration.Reason != "ACS_REGISTER" && migration.Reason != "ACS_CLOSE");
            if (migration.Reason == "ACS_REGISTER" && result.Accepted && MapStatus(result) == MigrationStatus.Active)
                card.Status = CardStatus.Registered;
            if (migration.Reason == "ACS_CLOSE" && result.Accepted && MapStatus(result) is MigrationStatus.Active or MigrationStatus.Submitted)
                card.Status = CardStatus.Closed;
            _store.Update(card);
        }
        catch (KeyNotFoundException)
        {
            migration.Status = MigrationStatus.ManualReview;
            migration.FailureReason = "No ACS data for correlation_id. Do not retry the same request id.";
        }
        catch (MastercardApiException ex) when (ex.StatusCode is 404 or 403)
        {
            migration.Status = MigrationStatus.ManualReview;
            migration.FailureReason = PanRedactor.Redact(ex.Message);
        }
        catch (AcsAmbiguousOutcomeException ex)
        {
            MarkUnknown(migration, ex);
        }

        _store.UpdateMigration(migration);
        return ToMigration(card, migration);
    }

    public async Task<int> ReconcileOpenAsync(CancellationToken cancellationToken = default)
    {
        var open = _store.ListNeedingReconcile();
        var count = 0;
        foreach (var migration in open)
        {
            await ReconcileAsync(migration.CardId, migration.MigrationId, cancellationToken);
            count++;
        }

        return count;
    }

    public async Task<MigrationResponse> RollbackAsync(
        string cardId,
        string migrationId,
        CancellationToken cancellationToken = default)
    {
        var original = _store.GetMigrationRequired(cardId, migrationId);
        if (original.Reason is "ACS_REGISTER" or "ACS_CLOSE")
            throw new EligibilityException("Register/close events cannot be rolled back with Product Graduation; close or re-register the PAN instead.");

        var rolled = await UpgradeAsync(
            cardId,
            new UpgradeCardRequest(original.SourceProductCode, "ROLLBACK", null),
            cancellationToken);

        original.Status = MigrationStatus.RolledBack;
        _store.UpdateMigration(original);
        return rolled;
    }

    public async Task<MigrationResponse> CloseAsync(
        string cardId,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        EnsureWritesEnabled();
        var card = _store.GetRequired(cardId);
        if (card.Status == CardStatus.Closed)
            throw new EligibilityException("Card is already closed.");

        var requestId = NewRequestId(correlationId);
        if (TryReplay(requestId, "ACS_CLOSE", card.ProductCode, out var replayed))
            return ToMigration(card, replayed);

        var migration = NewMigration(card, card.ProductCode, card.ProductCode, "ACS_CLOSE", requestId);
        try
        {
            var result = await _acs.DeleteRegistrationAsync(
                card.Pan,
                card.AcsProductRuleId,
                requestId,
                cancellationToken);
            ApplyWriteResult(card, migration, result, updateProduct: false);
            if (result.Accepted)
            {
                card.Status = CardStatus.Closed;
                card.AcsProductRuleId = null;
                _store.Update(card);
            }
        }
        catch (AcsAmbiguousOutcomeException ex)
        {
            MarkUnknown(migration, ex);
        }

        _store.AddMigration(migration);
        return ToMigration(_store.GetRequired(cardId), migration);
    }

    public TreatmentCheckResponse CheckTreatment(string cardId)
    {
        var card = _store.GetRequired(cardId);
        var open = _store.ListMigrations(cardId)
            .Where(m => OpenStatuses.Contains(m.Status))
            .Select(m => m.MigrationId)
            .ToList();

        string outcome;
        string summary;
        if (open.Count > 0)
        {
            outcome = "UNVERIFIED";
            summary = "Open ACS migrations exist (Submitted/Unknown). Do not treat issuer product as final; reconcile first.";
        }
        else if (string.IsNullOrWhiteSpace(card.NetworkProductCode))
        {
            outcome = "UNVERIFIED";
            summary = "No FINAL ACS product is stored yet. Register and wait for FINAL before authorization/clearing checks.";
        }
        else if (string.Equals(card.ProductCode, card.NetworkProductCode, StringComparison.OrdinalIgnoreCase))
        {
            outcome = "MATCH";
            summary = $"Issuer product {card.ProductCode} matches ACS Product Graduation product {card.NetworkProductCode}. Same PAN/BIN.";
        }
        else
        {
            outcome = "MISMATCH";
            summary = $"Issuer product {card.ProductCode} differs from ACS product {card.NetworkProductCode}. Repair before authorization.";
        }

        return new TreatmentCheckResponse(
            card.CardId,
            card.MaskedPan,
            card.ProductCode,
            card.NetworkProductCode,
            outcome,
            summary,
            open);
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
        var treatment = CheckTreatment(card.CardId);
        return new EndToEndDemoResult(
            latest,
            registration,
            upgrade,
            treatment,
            _acs.Mode,
            $"Created {latest.MaskedPan} as {source}, registered in PGP, upgraded {source} → {latest.ProductCode} with the same PAN/BIN. Treatment={treatment.Outcome}.");
    }

    private void EnsureWritesEnabled()
    {
        if (!_options.WritesEnabled)
            throw new KillSwitchException();
    }

    private bool TryReplay(string requestId, string reason, string targetProductCode, out ProductMigration migration)
    {
        var existing = _store.FindMigrationByCorrelationId(requestId);
        if (existing is null)
        {
            migration = null!;
            return false;
        }

        if (!string.Equals(existing.Reason, reason, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(existing.TargetProductCode, targetProductCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new IdempotencyConflictException(
                $"Request id '{requestId}' was already used for {existing.Reason} {existing.SourceProductCode}→{existing.TargetProductCode}.");
        }

        migration = existing;
        return true;
    }

    private static ProductMigration NewMigration(CardAccount card, string source, string target, string reason, string requestId) =>
        new()
        {
            MigrationId = $"mig_{Guid.NewGuid():N}"[..20],
            CardId = card.CardId,
            SourceProductCode = source,
            TargetProductCode = target,
            Reason = reason,
            CorrelationId = requestId,
            MastercardRequestId = requestId,
            SamePan = true,
            SameBin = true,
            Status = MigrationStatus.Created
        };

    private static string NewRequestId(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return Guid.NewGuid().ToString();

        if (correlationId.Length > 255 || correlationId.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-')))
            throw new EligibilityException("Correlation id must match Universal-Spec-Api-Request-Id: 1-255 of [0-9A-Za-z-].");

        return correlationId;
    }

    private static void ApplyWriteResult(CardAccount card, ProductMigration migration, AcsOperationResult result, bool updateProduct)
    {
        migration.MastercardRequestId = result.RequestId;
        migration.ProductRuleId = result.ProductRuleId ?? migration.ProductRuleId;
        migration.MastercardRawResponse = result.RawResponse;
        migration.Status = MapStatus(result);
        migration.FailureReason = result.Accepted ? null : result.Status;

        card.LastAcsRequestId = result.RequestId;
        if (!string.IsNullOrWhiteSpace(result.ProductRuleId))
            card.AcsProductRuleId = result.ProductRuleId;

        if (result.Accepted && migration.Status == MigrationStatus.Active)
        {
            if (updateProduct)
            {
                card.ProductCode = migration.TargetProductCode;
                card.NetworkProductCode = result.ProductCode ?? migration.TargetProductCode;
            }
            else if (migration.Reason == "ACS_REGISTER")
            {
                card.NetworkProductCode = result.ProductCode ?? card.ProductCode;
            }
        }
    }

    private static void MarkUnknown(ProductMigration migration, AcsAmbiguousOutcomeException ex)
    {
        migration.Status = MigrationStatus.Unknown;
        migration.FailureReason = ex.Message;
        migration.MastercardRawResponse = ex.ResponseBody;
    }

    private static MigrationStatus MapStatus(AcsOperationResult result)
    {
        if (!result.Accepted)
            return MigrationStatus.Rejected;

        if (string.Equals(result.ResponseIndicator, AcsResponseIndicators.Pending, StringComparison.OrdinalIgnoreCase))
            return MigrationStatus.Submitted;

        if (string.Equals(result.ResponseType, AcsResponseTypes.Final, StringComparison.OrdinalIgnoreCase))
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
            card.NetworkProductCode,
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
            PanRedactor.RedactPayload(migration.MastercardRawResponse),
            migration.FailureReason);
}
