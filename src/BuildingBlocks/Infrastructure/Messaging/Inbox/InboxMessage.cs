namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Inbox;

public class InboxMessage
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = default!;
    public DateTime ReceivedOn { get; init; }
    public DateTime? ProcessedOn { get; set; }
}
