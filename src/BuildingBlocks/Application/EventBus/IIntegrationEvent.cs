using MediatR;

namespace Aev.Integration.BuildingBlocks.Application.EventBus;

public interface IIntegrationEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
