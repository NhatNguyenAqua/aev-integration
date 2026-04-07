namespace Aev.Integration.ServiceRequest.IntegrationEvents;

public record ServiceRequestCreatedIntegrationEvent(
    Guid Id,
    string CustomerCode,
    string RequestType,
    DateTime OccurredOn);
