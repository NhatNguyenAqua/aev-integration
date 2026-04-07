using Autofac;
using Aev.Integration.BuildingBlocks.Infrastructure.Extensions;
using Aev.Integration.ServiceRequest.Application.Commands.CreateServiceRequest;
using Aev.Integration.ServiceRequest.Application.Queries.GetServiceRequests;
using Aev.Integration.ServiceRequest.Domain.ServiceRequest;
using Aev.Integration.ServiceRequest.Infrastructure.Persistence;
using Aev.Integration.ServiceRequest.Infrastructure.Persistence.Repositories;
using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.ServiceRequest.Infrastructure;

public class ServiceRequestModule : AutofacModuleBase
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<ServiceRequestDbContext>()
            .AsSelf()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterType<ServiceRequestRepository>()
            .As<IServiceRequestRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CreateServiceRequestCommandHandler>()
            .As<ICommandHandler<CreateServiceRequestCommand, Guid>>()
            .InstancePerLifetimeScope();

        builder.RegisterType<GetServiceRequestsQueryHandler>()
            .As<IQueryHandler<GetServiceRequestsQuery, IReadOnlyList<ServiceRequestDto>>>()
            .InstancePerLifetimeScope();
    }
}
