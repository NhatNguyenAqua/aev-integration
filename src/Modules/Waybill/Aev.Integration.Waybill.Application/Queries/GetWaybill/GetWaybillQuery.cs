using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.Waybill.Application.Queries.GetWaybill;

public record GetWaybillQuery(Guid WaybillId) : IQuery<WaybillDto?>;
