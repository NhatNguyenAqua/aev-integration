using Autofac;
using Aev.Integration.BuildingBlocks.Infrastructure.Extensions;
using Aev.Integration.Waybill.Application.Commands.CreateWaybill;
using Aev.Integration.Waybill.Application.Queries.GetWaybill;
using Aev.Integration.Waybill.Domain.Waybill;
using Aev.Integration.Waybill.Infrastructure.Persistence;
using Aev.Integration.Waybill.Infrastructure.Persistence.Repositories;
using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Waybill.Infrastructure;

public class WaybillModule : AutofacModuleBase
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<WaybillDbContext>()
            .AsSelf()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterType<WaybillRepository>()
            .As<IWaybillRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CreateWaybillCommandHandler>()
            .As<ICommandHandler<CreateWaybillCommand, Guid>>()
            .InstancePerLifetimeScope();

        builder.RegisterType<GetWaybillQueryHandler>()
            .As<IQueryHandler<GetWaybillQuery, WaybillDto?>>()
            .InstancePerLifetimeScope();
    }
}
