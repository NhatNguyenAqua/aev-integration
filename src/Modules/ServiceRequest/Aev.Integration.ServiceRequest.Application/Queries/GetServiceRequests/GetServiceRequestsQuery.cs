using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.ServiceRequest.Application.Queries.GetServiceRequests;

public record GetServiceRequestsQuery(string CustomerCode) : IQuery<IReadOnlyList<ServiceRequestDto>>;
