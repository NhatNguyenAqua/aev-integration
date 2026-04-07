namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;

public interface IInboxService
{
    Task<bool> IsAlreadyProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid eventId, string eventType, CancellationToken cancellationToken = default);
}
