using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;

public class ProcessInboxMessagesJob<TContext>(
    TContext dbContext,
    ILogger<ProcessInboxMessagesJob<TContext>> logger)
    where TContext : BaseDbContext
{
    private const int DefaultRetentionDays = 7;
    private readonly TimeSpan _retentionPeriod = TimeSpan.FromDays(DefaultRetentionDays);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(_retentionPeriod);

        var deleted = await dbContext.InboxMessages
            .Where(m => m.ProcessedOn != null && m.ProcessedOn < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            logger.LogInformation("Cleaned up {Count} processed inbox messages older than {Cutoff}",
                deleted, cutoff);
    }
}
