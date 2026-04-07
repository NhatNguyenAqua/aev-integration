namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Outbox;

public class OutboxMessage
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = default!;
    public string EventPayload { get; init; } = default!;
    public DateTime OccurredOn { get; init; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
}
