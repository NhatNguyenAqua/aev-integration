using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Waybill.Domain.Waybill;

public interface IWaybillRepository : IRepository<Waybill, Guid>
{
    Task<Waybill?> GetByWaybillNumberAsync(string waybillNumber, CancellationToken cancellationToken = default);
}
