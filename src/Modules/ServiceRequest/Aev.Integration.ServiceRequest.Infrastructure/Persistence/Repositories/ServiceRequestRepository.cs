using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Aev.Integration.ServiceRequest.Domain.ServiceRequest;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.ServiceRequest.Infrastructure.Persistence.Repositories;

public sealed class ServiceRequestRepository(ServiceRequestDbContext context)
    : BaseRepository<Domain.ServiceRequest.ServiceRequest, Guid, ServiceRequestDbContext>(context), IServiceRequestRepository
{
    public async Task<IReadOnlyList<Domain.ServiceRequest.ServiceRequest>> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
        => await Context.ServiceRequests
            .Where(x => x.CustomerCode == customerCode)
            .ToListAsync(cancellationToken);
}
