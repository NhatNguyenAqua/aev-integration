using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Aev.Integration.DataSync.Domain.SyncJob;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.DataSync.Infrastructure.Persistence.Repositories;

public sealed class SyncJobRepository(DataSyncDbContext context)
    : BaseRepository<SyncJob, Guid, DataSyncDbContext>(context), ISyncJobRepository
{
    public async Task<IReadOnlyList<SyncJob>> GetBySystemSourceAsync(string systemSource, CancellationToken cancellationToken = default)
    {
        var query = Context.SyncJobs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(systemSource))
            query = query.Where(x => x.SystemSource == systemSource);
        return await query.ToListAsync(cancellationToken);
    }
}
