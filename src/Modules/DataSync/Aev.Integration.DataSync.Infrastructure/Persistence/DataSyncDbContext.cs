using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Aev.Integration.DataSync.Domain.SyncJob;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.DataSync.Infrastructure.Persistence;

public sealed class DataSyncDbContext(DbContextOptions<DataSyncDbContext> options) : BaseDbContext(options)
{
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("data_sync");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataSyncDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
