using Aev.Integration.BuildingBlocks.Application.EventBus;
using Microsoft.Extensions.DependencyInjection;

namespace Aev.Integration.BuildingBlocks.Infrastructure.EventBus;

public class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        foreach (var handler in handlers)
            await handler.HandleAsync(integrationEvent, cancellationToken);
    }

    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var eventType = integrationEvent.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!;

        var tasks = handlers
            .Select(handler =>
            {
                var result = handleMethod.Invoke(handler, [integrationEvent, cancellationToken]);
                return result as Task ?? Task.CompletedTask;
            });

        return Task.WhenAll(tasks);
    }
}
