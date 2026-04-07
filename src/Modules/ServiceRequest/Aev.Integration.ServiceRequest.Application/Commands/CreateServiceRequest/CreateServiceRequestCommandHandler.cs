using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;
using Aev.Integration.ServiceRequest.Domain.ServiceRequest;
using ServiceRequestEntity = Aev.Integration.ServiceRequest.Domain.ServiceRequest.ServiceRequest;

namespace Aev.Integration.ServiceRequest.Application.Commands.CreateServiceRequest;

public sealed class CreateServiceRequestCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateServiceRequestCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateServiceRequestCommand command, CancellationToken cancellationToken = default)
    {
        var request = ServiceRequestEntity.Create(
            command.CustomerCode,
            command.CustomerType,
            command.RequestType,
            command.Description);

        await serviceRequestRepository.AddAsync(request, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return request.Id;
    }
}
