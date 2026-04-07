using MediatR;

namespace Aev.Integration.BuildingBlocks.Application.EventBus;

public interface IIntegrationEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IIntegrationEvent
{
}
