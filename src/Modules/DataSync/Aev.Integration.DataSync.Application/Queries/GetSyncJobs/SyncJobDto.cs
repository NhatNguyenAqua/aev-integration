namespace Aev.Integration.DataSync.Application.Queries.GetSyncJobs;

public record SyncJobDto(
    Guid Id,
    string SystemSource,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int RecordsSynced);
