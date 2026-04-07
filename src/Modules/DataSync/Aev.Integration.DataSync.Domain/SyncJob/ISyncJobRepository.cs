using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.DataSync.Domain.SyncJob;

public interface ISyncJobRepository : IRepository<SyncJob, Guid>
{
    Task<IReadOnlyList<SyncJob>> GetBySystemSourceAsync(string systemSource, CancellationToken cancellationToken = default);
}
