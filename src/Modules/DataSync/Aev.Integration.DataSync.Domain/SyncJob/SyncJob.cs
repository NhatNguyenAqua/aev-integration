using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.DataSync.Domain.SyncJob;

public class SyncJob : AggregateRoot<Guid>
{
    public string SystemSource { get; private set; } = default!;
    public SyncJobStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int RecordsSynced { get; private set; }
    public string? ErrorMessage { get; private set; }

    private SyncJob() { }

    public static SyncJob Create(string systemSource)
    {
        var job = new SyncJob
        {
            Id = Guid.NewGuid(),
            SystemSource = systemSource,
            Status = SyncJobStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        job.AddDomainEvent(new SyncJobCreatedDomainEvent(job.Id, systemSource));
        return job;
    }

    public void Complete(int recordsSynced)
    {
        Status = SyncJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        RecordsSynced = recordsSynced;
        IncrementVersion();
    }

    public void Fail(string errorMessage)
    {
        Status = SyncJobStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        IncrementVersion();
    }
}
