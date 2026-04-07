using Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;
using Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Outbox;
using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Hangfire;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Extensions;

public static class HangfireOutboxExtensions
{
    /// <summary>
    /// Registers recurring Hangfire jobs to process the outbox (every minute) and
    /// clean up the inbox (daily) for the specified module DbContext.
    /// Call this from each module's startup registration (e.g., after app.Build()).
    /// </summary>
    /// <param name="recurringJobManager">The Hangfire recurring job manager.</param>
    /// <param name="moduleId">A unique identifier for the module (e.g. "datasync").</param>
    /// <param name="outboxCron">Cron expression for outbox processing. Defaults to every minute.</param>
    /// <param name="inboxCleanupCron">Cron expression for inbox cleanup. Defaults to daily.</param>
    public static IRecurringJobManager AddOutboxProcessing<TContext>(
        this IRecurringJobManager recurringJobManager,
        string moduleId,
        string? outboxCron = null,
        string? inboxCleanupCron = null)
        where TContext : BaseDbContext
    {
        recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob<TContext>>(
            $"{moduleId}-process-outbox",
            job => job.ExecuteAsync(CancellationToken.None),
            outboxCron ?? Cron.Minutely());

        recurringJobManager.AddOrUpdate<ProcessInboxMessagesJob<TContext>>(
            $"{moduleId}-cleanup-inbox",
            job => job.ExecuteAsync(CancellationToken.None),
            inboxCleanupCron ?? Cron.Daily());

        return recurringJobManager;
    }
}
