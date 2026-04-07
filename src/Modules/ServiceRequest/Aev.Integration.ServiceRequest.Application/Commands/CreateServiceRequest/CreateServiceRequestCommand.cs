using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.ServiceRequest.Domain.ServiceRequest;

namespace Aev.Integration.ServiceRequest.Application.Commands.CreateServiceRequest;

public record CreateServiceRequestCommand(
    string CustomerCode,
    CustomerType CustomerType,
    string RequestType,
    string Description) : ICommand<Guid>;
