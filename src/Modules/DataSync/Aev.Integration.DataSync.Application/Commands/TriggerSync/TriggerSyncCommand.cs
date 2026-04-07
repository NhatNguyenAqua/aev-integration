using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.DataSync.Application.Commands.TriggerSync;

public record TriggerSyncCommand(string SystemSource) : ICommand<Guid>;
