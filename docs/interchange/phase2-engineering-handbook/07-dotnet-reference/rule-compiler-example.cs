using System.Collections.Immutable;

namespace Interchange.Engine;

public sealed record CompiledRule(
    string Id,
    string ProgramId,
    int Priority,
    int Specificity,
    Func<NormalizedTransaction, bool> Predicate,
    RateDefinition Rate);

public sealed class RuleCompiler
{
    private readonly IConditionCompiler _conditions;

    public RuleCompiler(IConditionCompiler conditions) =>
        _conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));

    public CompiledRule Compile(RuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var predicate = _conditions.Compile(rule.Conditions);
        var specificity = SpecificityCalculator.Calculate(rule.Conditions);

        return new CompiledRule(
            rule.Id,
            rule.ProgramId,
            rule.Priority,
            specificity,
            predicate,
            rule.Rate);
    }

    public ImmutableArray<CompiledRule> CompilePartition(
        IEnumerable<RuleDefinition> rules) =>
        rules.Select(Compile)
             .OrderByDescending(x => x.Priority)
             .ThenByDescending(x => x.Specificity)
             .ThenBy(x => x.Id, StringComparer.Ordinal)
             .ToImmutableArray();
}
