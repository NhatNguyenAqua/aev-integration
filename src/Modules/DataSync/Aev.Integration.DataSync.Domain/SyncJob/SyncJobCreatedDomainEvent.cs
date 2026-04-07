using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.DataSync.Domain.SyncJob;

public record SyncJobCreatedDomainEvent(Guid SyncJobId, string SystemSource) : DomainEvent;
