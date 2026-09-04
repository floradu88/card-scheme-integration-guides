using System.Collections.Concurrent;
using MastercardCardUpgrade.Api.Models.Cards;

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
    IReadOnlyList<ProductMigration> ListMigrations(string cardId);
    void UpdateMigration(ProductMigration migration);
}

public sealed class InMemoryCardStore : ICardStore
{
    private readonly ConcurrentDictionary<string, CardAccount> _cards = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ProductMigration> _migrations = new(StringComparer.OrdinalIgnoreCase);

    public CardAccount Add(CardAccount card)
    {
        if (!_cards.TryAdd(card.CardId, card))
            throw new InvalidOperationException($"Card '{card.CardId}' already exists.");
        return card;
    }

    public CardAccount GetRequired(string cardId) =>
        _cards.TryGetValue(cardId, out var card)
            ? card
            : throw new KeyNotFoundException($"Card '{cardId}' was not found.");

    public CardAccount? FindByPan(string pan) =>
        _cards.Values.FirstOrDefault(c => c.Pan == pan);

    public IReadOnlyList<CardAccount> List() =>
        _cards.Values.OrderByDescending(c => c.CreatedAt).ToList();

    public void Update(CardAccount card)
    {
        card.UpdatedAt = DateTimeOffset.UtcNow;
        _cards[card.CardId] = card;
    }

    public ProductMigration AddMigration(ProductMigration migration)
    {
        _migrations[migration.MigrationId] = migration;
        return migration;
    }

    public ProductMigration GetMigrationRequired(string cardId, string migrationId)
    {
        if (!_migrations.TryGetValue(migrationId, out var migration) ||
            !string.Equals(migration.CardId, cardId, StringComparison.OrdinalIgnoreCase))
            throw new KeyNotFoundException($"Migration '{migrationId}' was not found for card '{cardId}'.");
        return migration;
    }

    public IReadOnlyList<ProductMigration> ListMigrations(string cardId) =>
        _migrations.Values
            .Where(m => string.Equals(m.CardId, cardId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

    public void UpdateMigration(ProductMigration migration)
    {
        migration.UpdatedAt = DateTimeOffset.UtcNow;
        _migrations[migration.MigrationId] = migration;
    }
}
