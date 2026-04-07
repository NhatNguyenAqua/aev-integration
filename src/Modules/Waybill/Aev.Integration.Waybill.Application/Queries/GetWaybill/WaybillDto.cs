namespace Aev.Integration.Waybill.Application.Queries.GetWaybill;

public record WaybillDto(
    Guid Id,
    string WaybillNumber,
    string LogisticsProvider,
    string SenderAddress,
    string ReceiverAddress,
    string Status,
    DateTime CreatedAt);
