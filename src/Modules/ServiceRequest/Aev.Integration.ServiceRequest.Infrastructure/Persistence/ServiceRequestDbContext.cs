using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.ServiceRequest.Infrastructure.Persistence;

public sealed class ServiceRequestDbContext(DbContextOptions<ServiceRequestDbContext> options) : BaseDbContext(options)
{
    public DbSet<Domain.ServiceRequest.ServiceRequest> ServiceRequests => Set<Domain.ServiceRequest.ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("service_request");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceRequestDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
