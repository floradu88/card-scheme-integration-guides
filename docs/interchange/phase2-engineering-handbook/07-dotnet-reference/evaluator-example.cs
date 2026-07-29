namespace Interchange.Engine;

public sealed class InterchangeEvaluator : IInterchangeEngine
{
    private readonly IConfigurationSnapshotProvider _snapshots;
    private readonly IFeeCalculator _fees;
    private readonly IContextDeriver _deriver;

    public InterchangeEvaluator(
        IConfigurationSnapshotProvider snapshots,
        IFeeCalculator fees,
        IContextDeriver deriver)
    {
        _snapshots = snapshots;
        _fees = fees;
        _deriver = deriver;
    }

    public QualificationDecision Evaluate(
        NormalizedTransaction transaction,
        EvaluationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        var context = _deriver.Derive(transaction);
        var snapshot = _snapshots.GetSnapshot(
            context.Network,
            context.Region,
            context.EventTimestamp);

        var partition = snapshot.FindPartition(PartitionKey.From(context));

        foreach (var rule in partition.Candidates)
        {
            if (!rule.Predicate(context))
                continue;

            var fee = _fees.Calculate(rule.Rate, context.Amount);

            return QualificationDecisionFactory.Match(
                snapshot.Version,
                rule,
                fee);
        }

        return QualificationDecisionFactory.Unmatched(
            snapshot.Version,
            partition.UnmatchedPolicy);
    }
}
