using System.Text.Json;
using Aev.Integration.BuildingBlocks.Application.EventBus;
using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Aev.Integration.BuildingBlocks.Infrastructure.Messaging.Outbox;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob<TContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<ProcessOutboxMessagesJob<TContext>> logger)
    : IJob where TContext : BaseDbContext
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(50)
            .ToListAsync(context.CancellationToken);

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

                await eventBus.PublishAsync(integrationEvent, context.CancellationToken);

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

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
