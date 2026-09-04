using MastercardCardUpgrade.Api.Options;
using Microsoft.Extensions.Options;

namespace MastercardCardUpgrade.Api.Services;

public sealed class AcsReconcileWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly MastercardOptions _options;
    private readonly ILogger<AcsReconcileWorker> _log;

    public AcsReconcileWorker(
        IServiceScopeFactory scopes,
        IOptions<MastercardOptions> options,
        ILogger<AcsReconcileWorker> log)
    {
        _scopes = scopes;
        _options = options.Value;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.ReconcileIntervalSeconds <= 0)
            return;

        var delay = TimeSpan.FromSeconds(Math.Max(5, _options.ReconcileIntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var lifecycle = scope.ServiceProvider.GetRequiredService<ICardLifecycleService>();
                var n = await lifecycle.ReconcileOpenAsync(stoppingToken);
                if (n > 0)
                    _log.LogInformation("Reconciled {Count} open ACS migrations.", n);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "ACS reconcile worker cycle failed.");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
