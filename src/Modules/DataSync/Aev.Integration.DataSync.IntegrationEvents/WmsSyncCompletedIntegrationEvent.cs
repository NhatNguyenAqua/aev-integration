namespace Aev.Integration.DataSync.IntegrationEvents;

public record WmsSyncCompletedIntegrationEvent(
    Guid Id,
    string SystemSource,
    DateTime OccurredOn,
    int RecordsSynced);
