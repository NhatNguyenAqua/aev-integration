using Aev.Integration.BuildingBlocks.Application.EventBus;
using MediatR;

namespace Aev.Integration.BuildingBlocks.Infrastructure.EventBus;

public class InMemoryEventBus(IPublisher publisher) : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
        => publisher.Publish(integrationEvent, cancellationToken);
}
