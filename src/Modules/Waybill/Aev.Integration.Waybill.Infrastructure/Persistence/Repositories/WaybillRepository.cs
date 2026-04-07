using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Aev.Integration.Waybill.Domain.Waybill;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.Waybill.Infrastructure.Persistence.Repositories;

public sealed class WaybillRepository(WaybillDbContext context)
    : BaseRepository<Domain.Waybill.Waybill, Guid, WaybillDbContext>(context), IWaybillRepository
{
    public async Task<Domain.Waybill.Waybill?> GetByWaybillNumberAsync(string waybillNumber, CancellationToken cancellationToken = default)
        => await Context.Waybills.FirstOrDefaultAsync(x => x.WaybillNumber == waybillNumber, cancellationToken);
}
