using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.Waybill.Domain.Waybill;

namespace Aev.Integration.Waybill.Application.Queries.GetWaybill;

public sealed class GetWaybillQueryHandler(IWaybillRepository waybillRepository)
    : IQueryHandler<GetWaybillQuery, WaybillDto?>
{
    public async Task<WaybillDto?> HandleAsync(GetWaybillQuery query, CancellationToken cancellationToken = default)
    {
        var waybill = await waybillRepository.GetByIdAsync(query.WaybillId, cancellationToken);

        return waybill is null ? null : new WaybillDto(
            waybill.Id,
            waybill.WaybillNumber,
            waybill.LogisticsProvider,
            waybill.SenderAddress,
            waybill.ReceiverAddress,
            waybill.Status.ToString(),
            waybill.CreatedAt);
    }
}
