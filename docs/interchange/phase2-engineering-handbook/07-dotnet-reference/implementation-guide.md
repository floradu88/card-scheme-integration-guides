# .NET Reference Implementation Guide

## Suggested projects

```text
Interchange.Domain
Interchange.Configuration
Interchange.Compiler
Interchange.Engine
Interchange.Adapters.Visa
Interchange.Adapters.Mastercard
Interchange.Persistence
Interchange.ControlPlane.Api
Interchange.Reconciliation.Worker
Interchange.Admin.Web
Interchange.Tests
Interchange.Benchmarks
```

## Core types

```csharp
public sealed record Money(long MinorUnits, string Currency);

public sealed record NormalizedTransaction(
    string TransactionId,
    DateTimeOffset EventTimestamp,
    Network Network,
    Money Amount,
    MerchantContext Merchant,
    CardContext Card,
    AcceptanceContext Acceptance,
    AuthenticationContext Authentication,
    TimingContext Timing,
    IReadOnlyDictionary<string, string> Extensions);

public sealed record QualificationDecision(
    Guid DecisionId,
    string ConfigurationVersion,
    string? ProgramId,
    string? RuleId,
    Money? EstimatedInterchange,
    bool IsFallback,
    IReadOnlyList<string> ReasonCodes);
```

## Engine interface

```csharp
public interface IInterchangeEngine
{
    QualificationDecision Evaluate(
        NormalizedTransaction transaction,
        EvaluationOptions? options = null);
}
```

## Snapshot provider

```csharp
public interface IConfigurationSnapshotProvider
{
    CompiledSnapshot GetSnapshot(
        Network network,
        string region,
        DateTimeOffset eventTimestamp);
}
```

## Atomic update

Use one reference to the immutable snapshot set:

```csharp
private SnapshotSet _current = SnapshotSet.Empty;

public SnapshotSet Current => Volatile.Read(ref _current);

public void Activate(SnapshotSet next)
{
    ArgumentNullException.ThrowIfNull(next);
    Interlocked.Exchange(ref _current, next);
}
```

## Decimal safety

Calculate percentages with `decimal`, then round under the configured policy. Avoid `double`.

## Dependency injection

The data plane should register the engine and snapshot provider as singletons. Adapters may be scoped/transient depending on parser state.
