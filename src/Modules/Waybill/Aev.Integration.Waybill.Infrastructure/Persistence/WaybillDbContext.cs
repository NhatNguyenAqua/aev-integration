using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.Waybill.Infrastructure.Persistence;

public sealed class WaybillDbContext(DbContextOptions<WaybillDbContext> options) : BaseDbContext(options)
{
    public DbSet<Domain.Waybill.Waybill> Waybills => Set<Domain.Waybill.Waybill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("waybill");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WaybillDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
