using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.DataSync.Application.Queries.GetSyncJobs;

public record GetSyncJobsQuery(string? SystemSource) : IQuery<IReadOnlyList<SyncJobDto>>;
