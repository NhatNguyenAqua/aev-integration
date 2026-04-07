using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.ServiceRequest.Domain.ServiceRequest;

namespace Aev.Integration.ServiceRequest.Application.Queries.GetServiceRequests;

public sealed class GetServiceRequestsQueryHandler(IServiceRequestRepository serviceRequestRepository)
    : IQueryHandler<GetServiceRequestsQuery, IReadOnlyList<ServiceRequestDto>>
{
    public async Task<IReadOnlyList<ServiceRequestDto>> HandleAsync(GetServiceRequestsQuery query, CancellationToken cancellationToken = default)
    {
        var requests = await serviceRequestRepository.GetByCustomerCodeAsync(query.CustomerCode, cancellationToken);

        return requests.Select(r => new ServiceRequestDto(
            r.Id,
            r.CustomerCode,
            r.CustomerType.ToString(),
            r.RequestType,
            r.Description,
            r.Status.ToString(),
            r.CreatedAt)).ToList();
    }
}
