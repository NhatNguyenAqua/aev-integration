using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.Waybill.Application.Commands.CreateWaybill;

public record CreateWaybillCommand(
    string WaybillNumber,
    string SenderAddress,
    string ReceiverAddress) : ICommand<Guid>;
