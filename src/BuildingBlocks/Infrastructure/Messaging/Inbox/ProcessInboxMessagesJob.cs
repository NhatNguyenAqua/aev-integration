using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;

[DisallowConcurrentExecution]
public class ProcessInboxMessagesJob<TContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<ProcessInboxMessagesJob<TContext>> logger)
    : IJob where TContext : BaseDbContext
{
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        var cutoff = DateTime.UtcNow.Subtract(RetentionPeriod);

        var deleted = await dbContext.InboxMessages
            .Where(m => m.ProcessedOn != null && m.ProcessedOn < cutoff)
            .ExecuteDeleteAsync(context.CancellationToken);

        if (deleted > 0)
            logger.LogInformation("Cleaned up {Count} processed inbox messages older than {Cutoff}",
                deleted, cutoff);
    }
}
