using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.DataSync.Domain.SyncJob;

namespace Aev.Integration.DataSync.Application.Queries.GetSyncJobs;

public sealed class GetSyncJobsQueryHandler(ISyncJobRepository syncJobRepository)
    : IQueryHandler<GetSyncJobsQuery, IReadOnlyList<SyncJobDto>>
{
    public async Task<IReadOnlyList<SyncJobDto>> HandleAsync(GetSyncJobsQuery query, CancellationToken cancellationToken = default)
    {
        var jobs = query.SystemSource is not null
            ? await syncJobRepository.GetBySystemSourceAsync(query.SystemSource, cancellationToken)
            : await syncJobRepository.GetBySystemSourceAsync(string.Empty, cancellationToken);

        return jobs.Select(j => new SyncJobDto(
            j.Id,
            j.SystemSource,
            j.Status.ToString(),
            j.CreatedAt,
            j.CompletedAt,
            j.RecordsSynced)).ToList();
    }
}
