using System.Collections.Concurrent;
using System.Text.Json;
using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public interface ICardStore
{
    CardAccount Add(CardAccount card);
    CardAccount GetRequired(string cardId);
    CardAccount? FindByPan(string pan);
    IReadOnlyList<CardAccount> List();
    void Update(CardAccount card);
    ProductMigration AddMigration(ProductMigration migration);
    ProductMigration GetMigrationRequired(string cardId, string migrationId);
    ProductMigration? FindMigrationByCorrelationId(string correlationId);
    IReadOnlyList<ProductMigration> ListMigrations(string cardId);
    IReadOnlyList<ProductMigration> ListNeedingReconcile();
    void UpdateMigration(ProductMigration migration);
}

public sealed class InMemoryCardStore : ICardStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly ConcurrentDictionary<string, CardAccount> _cards = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ProductMigration> _migrations = new(StringComparer.OrdinalIgnoreCase);
    private readonly string? _path;
    private readonly object _gate = new();

    public InMemoryCardStore(IOptions<MastercardOptions> options)
    {
        var configured = options.Value.CardStorePath;
        _path = string.IsNullOrWhiteSpace(configured) ? null : configured;
        Load();
    }

    public CardAccount Add(CardAccount card)
    {
        if (!_cards.TryAdd(card.CardId, card))
            throw new InvalidOperationException($"Card '{card.CardId}' already exists.");
        Persist();
        return card;
    }

    public CardAccount GetRequired(string cardId) =>
        _cards.TryGetValue(cardId, out var card)
            ? card
            : throw new KeyNotFoundException($"Card '{cardId}' was not found.");

    public CardAccount? FindByPan(string pan) =>
        _cards.Values.FirstOrDefault(c => c.Pan == pan);

    public IReadOnlyList<CardAccount> List() => _cards.Values.OrderByDescending(c => c.CreatedAt).ToList();

    public void Update(CardAccount card)
    {
        card.UpdatedAt = DateTimeOffset.UtcNow;
        _cards[card.CardId] = card;
        Persist();
    }

    public ProductMigration AddMigration(ProductMigration migration)
    {
        _migrations[migration.MigrationId] = migration;
        Persist();
        return migration;
    }

    public ProductMigration GetMigrationRequired(string cardId, string migrationId)
    {
        if (!_migrations.TryGetValue(migrationId, out var migration) ||
            !string.Equals(migration.CardId, cardId, StringComparison.OrdinalIgnoreCase))
            throw new KeyNotFoundException($"Migration '{migrationId}' was not found for card '{cardId}'.");
        return migration;
    }

    public ProductMigration? FindMigrationByCorrelationId(string correlationId) =>
        string.IsNullOrWhiteSpace(correlationId)
            ? null
            : _migrations.Values.FirstOrDefault(m =>
                string.Equals(m.CorrelationId, correlationId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(m.MastercardRequestId, correlationId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ProductMigration> ListMigrations(string cardId) =>
        _migrations.Values
            .Where(m => string.Equals(m.CardId, cardId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

    public IReadOnlyList<ProductMigration> ListNeedingReconcile() =>
        _migrations.Values
            .Where(m => m.Status is MigrationStatus.Submitted or MigrationStatus.Unknown or MigrationStatus.Reconciling or MigrationStatus.Accepted)
            .OrderBy(m => m.UpdatedAt)
            .ToList();

    public void UpdateMigration(ProductMigration migration)
    {
        migration.UpdatedAt = DateTimeOffset.UtcNow;
        _migrations[migration.MigrationId] = migration;
        Persist();
    }

    private void Load()
    {
        if (_path is null || !File.Exists(_path))
            return;

        var snapshot = JsonSerializer.Deserialize<CardStoreSnapshot>(File.ReadAllText(_path), JsonOptions);
        if (snapshot is null)
            return;

        foreach (var card in snapshot.Cards)
            _cards[card.CardId] = card;
        foreach (var migration in snapshot.Migrations)
            _migrations[migration.MigrationId] = migration;
    }

    private void Persist()
    {
        if (_path is null)
            return;

        lock (_gate)
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var snapshot = new CardStoreSnapshot
            {
                Cards = _cards.Values.ToList(),
                Migrations = _migrations.Values.ToList()
            };
            File.WriteAllText(_path, JsonSerializer.Serialize(snapshot, JsonOptions));
        }
    }

    private sealed class CardStoreSnapshot
    {
        public List<CardAccount> Cards { get; set; } = [];
        public List<ProductMigration> Migrations { get; set; } = [];
    }
}
