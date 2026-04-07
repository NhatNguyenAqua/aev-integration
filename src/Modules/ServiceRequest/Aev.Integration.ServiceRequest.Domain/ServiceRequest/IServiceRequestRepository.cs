using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.ServiceRequest.Domain.ServiceRequest;

public interface IServiceRequestRepository : IRepository<ServiceRequest, Guid>
{
    Task<IReadOnlyList<ServiceRequest>> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default);
}
