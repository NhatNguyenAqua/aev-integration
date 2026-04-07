namespace Aev.Integration.Waybill.IntegrationEvents;

public record WaybillCreatedIntegrationEvent(
    Guid Id,
    string WaybillNumber,
    string LogisticsProvider,
    DateTime OccurredOn);
