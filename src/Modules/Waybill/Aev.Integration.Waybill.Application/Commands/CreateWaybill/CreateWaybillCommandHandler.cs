using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;
using Aev.Integration.Waybill.Domain.Waybill;
using WaybillEntity = Aev.Integration.Waybill.Domain.Waybill.Waybill;

namespace Aev.Integration.Waybill.Application.Commands.CreateWaybill;

public sealed class CreateWaybillCommandHandler(
    IWaybillRepository waybillRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateWaybillCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateWaybillCommand command, CancellationToken cancellationToken = default)
    {
        var waybill = WaybillEntity.Create(command.WaybillNumber, command.SenderAddress, command.ReceiverAddress);

        await waybillRepository.AddAsync(waybill, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return waybill.Id;
    }
}
