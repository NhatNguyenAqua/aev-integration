using Autofac;
using Aev.Integration.BuildingBlocks.Infrastructure.Extensions;
using Aev.Integration.DataSync.Application.Commands.TriggerSync;
using Aev.Integration.DataSync.Application.Queries.GetSyncJobs;
using Aev.Integration.DataSync.Domain.SyncJob;
using Aev.Integration.DataSync.Infrastructure.Persistence;
using Aev.Integration.DataSync.Infrastructure.Persistence.Repositories;
using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.DataSync.Infrastructure;

public class DataSyncModule : AutofacModuleBase
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<DataSyncDbContext>()
            .AsSelf()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterType<SyncJobRepository>()
            .As<ISyncJobRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TriggerSyncCommandHandler>()
            .As<ICommandHandler<TriggerSyncCommand, Guid>>()
            .InstancePerLifetimeScope();

        builder.RegisterType<GetSyncJobsQueryHandler>()
            .As<IQueryHandler<GetSyncJobsQuery, IReadOnlyList<SyncJobDto>>>()
            .InstancePerLifetimeScope();
    }
}
