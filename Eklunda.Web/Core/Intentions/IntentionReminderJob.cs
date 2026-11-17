using Eklunda.Web.Shared;

namespace Eklunda.Web.Core.Intentions;

public class IntentionReminderJob(
    IServiceScopeFactory scopeFactory,
    ILogger<IntentionReminderJob> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);
        do
        {
            try
            {
                await SendDue(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Sending intention reminders failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SendDue(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var intentionService = scope.ServiceProvider.GetRequiredService<IntentionService>();
        var intentionNotifier = scope.ServiceProvider.GetRequiredService<IntentionNotifier>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTimeOffset.UtcNow;
        foreach (var intention in await intentionService.GetDueReminders(now, cancellationToken))
        {
            await intentionNotifier.NotifyReminder(intention, cancellationToken);
            intention.ReminderSent(now);
            await unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
