using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.ServiceRequest.Domain.ServiceRequest;

public record ServiceRequestCreatedDomainEvent(Guid RequestId, string CustomerCode, string RequestType) : DomainEvent;
