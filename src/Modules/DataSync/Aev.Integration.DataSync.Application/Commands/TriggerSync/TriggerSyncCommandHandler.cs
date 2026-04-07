using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;
using Aev.Integration.DataSync.Domain.SyncJob;

namespace Aev.Integration.DataSync.Application.Commands.TriggerSync;

public sealed class TriggerSyncCommandHandler(
    ISyncJobRepository syncJobRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TriggerSyncCommand, Guid>
{
    public async Task<Guid> HandleAsync(TriggerSyncCommand command, CancellationToken cancellationToken = default)
    {
        var syncJob = SyncJob.Create(command.SystemSource);

        await syncJobRepository.AddAsync(syncJob, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return syncJob.Id;
    }
}
