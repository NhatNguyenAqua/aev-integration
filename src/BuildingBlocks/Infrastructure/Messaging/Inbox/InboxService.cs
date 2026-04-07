using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;

public class InboxService<TContext>(TContext context) : IInboxService
    where TContext : BaseDbContext
{
    public async Task<bool> IsAlreadyProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        => await context.InboxMessages
            .AnyAsync(m => m.Id == eventId && m.ProcessedOn != null, cancellationToken);

    public async Task MarkAsProcessedAsync(Guid eventId, string eventType, CancellationToken cancellationToken = default)
    {
        var existing = await context.InboxMessages
            .FirstOrDefaultAsync(m => m.Id == eventId, cancellationToken);

        if (existing is null)
        {
            context.InboxMessages.Add(new InboxMessage
            {
                Id = eventId,
                EventType = eventType,
                ReceivedOn = DateTime.UtcNow,
                ProcessedOn = DateTime.UtcNow
            });
        }
        else
        {
            existing.ProcessedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
