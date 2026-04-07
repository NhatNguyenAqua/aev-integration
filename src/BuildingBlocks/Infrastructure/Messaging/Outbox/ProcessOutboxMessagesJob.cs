using System.Text.Json;
using Aev.Integration.BuildingBlocks.Application.EventBus;
using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Outbox;

public class ProcessOutboxMessagesJob<TContext>(
    TContext dbContext,
    IEventBus eventBus,
    ILogger<ProcessOutboxMessagesJob<TContext>> logger)
    where TContext : BaseDbContext
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int BatchSize = 50;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    logger.LogWarning("Could not resolve type {EventType} for outbox message {MessageId}",
                        message.EventType, message.Id);
                    message.Error = $"Could not resolve type: {message.EventType}";
                    message.ProcessedOn = DateTime.UtcNow;
                    continue;
                }

                var integrationEvent = (IIntegrationEvent?)JsonSerializer.Deserialize(
                    message.EventPayload, eventType, SerializerOptions);

                if (integrationEvent is null)
                {
                    logger.LogWarning("Could not deserialize outbox message {MessageId}", message.Id);
                    message.Error = "Deserialization returned null";
                    message.ProcessedOn = DateTime.UtcNow;
                    continue;
                }

                await eventBus.PublishAsync(integrationEvent, cancellationToken);

                message.ProcessedOn = DateTime.UtcNow;

                logger.LogDebug("Published outbox message {MessageId} of type {EventType}",
                    message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                message.Error = ex.Message;
                message.ProcessedOn = DateTime.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
