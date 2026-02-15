using BOG.BL.Interfaces.CaseRegistration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BOG.API.BackgroundServices;

/// <summary>
/// Background service that periodically checks for expired completion requests.
/// Implements BR05: Auto-rejection after 30 days if applicant doesn't submit documents.
///
/// This service:
/// 1. Runs daily (by default at midnight)
/// 2. Queries all requests in PendingCompletion state with expired deadlines
/// 3. Auto-rejects them with appropriate notifications
/// 4. Logs all operations for monitoring
/// </summary>
public class CompletionDeadlineCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CompletionDeadlineCheckerService> _logger;

    public CompletionDeadlineCheckerService(
        IServiceProvider serviceProvider,
        ILogger<CompletionDeadlineCheckerService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the background service.
    /// Runs continuously and processes expired requests once per day.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CompletionDeadlineCheckerService started");

        // Wait for application to fully start up before first database query
        _logger.LogInformation("Waiting 10 seconds for database initialization...");
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        _logger.LogInformation("Starting CompletionDeadlineCheckerService processing loop");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Calculate delay until next execution
                var now = DateTime.Now;
                var tomorrow = now.Date.AddDays(1);
                var delayUntilNextRun = tomorrow - now;

                _logger.LogDebug("Next scheduled run at {NextRunTime}", tomorrow);

                // Wait until the scheduled time (or until cancellation requested)
                await Task.Delay(delayUntilNextRun, stoppingToken);

                // Process expired requests
                await ProcessExpiredRequestsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CompletionDeadlineCheckerService cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CompletionDeadlineCheckerService");

                // Wait 1 hour before retrying on error
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("CompletionDeadlineCheckerService stopped");
    }

    /// <summary>
    /// Processes all expired completion requests.
    /// </summary>
    private async Task ProcessExpiredRequestsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var requestActionBL = scope.ServiceProvider.GetRequiredService<IRequestActionBL>();

        _logger.LogInformation("Starting to process expired completion requests");

        try
        {
            await requestActionBL.ProcessExpiredCompletionRequestsAsync(cancellationToken);
            _logger.LogInformation("Successfully processed expired completion requests");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired completion requests");
            throw;
        }
    }

    /// <summary>
    /// Stops the background service.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CompletionDeadlineCheckerService stopping");
        await base.StopAsync(cancellationToken);
    }
}
