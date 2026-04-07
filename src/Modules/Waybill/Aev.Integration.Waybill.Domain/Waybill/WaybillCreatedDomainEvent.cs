using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Waybill.Domain.Waybill;

public record WaybillCreatedDomainEvent(Guid WaybillId, string WaybillNumber) : DomainEvent;
