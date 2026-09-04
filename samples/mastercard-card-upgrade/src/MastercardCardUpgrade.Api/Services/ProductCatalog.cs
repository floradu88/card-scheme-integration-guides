using MastercardCardUpgrade.Api.Models.Cards;
using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public interface IProductCatalog
{
    IReadOnlyList<ProductDefinition> Products { get; }
    ProductDefinition GetRequired(string productCode);
    bool IsAllowedTransition(string fromProductCode, string toProductCode);
    bool IsAllowedAccountRange(string pan);
}

public sealed class ProductCatalog : IProductCatalog
{
    private readonly Dictionary<string, ProductDefinition> _products;
    private readonly Dictionary<string, HashSet<string>> _transitions;
    private readonly List<string> _accountRangePrefixes;

    public ProductCatalog(IOptions<ProductCatalogOptions> options)
    {
        var cfg = options.Value;
        _products = cfg.Products.ToDictionary(p => p.Code, p => p, StringComparer.OrdinalIgnoreCase);
        _transitions = cfg.AllowedTransitions.ToDictionary(
            t => t.From,
            t => t.To.ToHashSet(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        _accountRangePrefixes = cfg.AllowedAccountRangePrefixes
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => new string(p.Where(char.IsDigit).ToArray()))
            .Where(p => p.Length > 0)
            .ToList();
    }

    public IReadOnlyList<ProductDefinition> Products => _products.Values.ToList();

    public ProductDefinition GetRequired(string productCode)
    {
        if (_products.TryGetValue(productCode, out var product))
            return product;

        throw new EligibilityException($"Unknown product code '{productCode}'.");
    }

    public bool IsAllowedTransition(string fromProductCode, string toProductCode) =>
        _transitions.TryGetValue(fromProductCode, out var targets)
        && targets.Contains(toProductCode);

    public bool IsAllowedAccountRange(string pan)
    {
        if (_accountRangePrefixes.Count == 0)
            return true;

        return _accountRangePrefixes.Any(prefix => pan.StartsWith(prefix, StringComparison.Ordinal));
    }
}
