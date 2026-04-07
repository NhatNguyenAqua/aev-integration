using Autofac;
using Aev.Integration.BuildingBlocks.Infrastructure.Extensions;
using Aev.Integration.Invoicing.Application.Commands.CreateInvoice;
using Aev.Integration.Invoicing.Application.Queries.GetInvoice;
using Aev.Integration.Invoicing.Domain.Invoice;
using Aev.Integration.Invoicing.Infrastructure.Persistence;
using Aev.Integration.Invoicing.Infrastructure.Persistence.Repositories;
using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Invoicing.Infrastructure;

public class InvoicingModule : AutofacModuleBase
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<InvoicingDbContext>()
            .AsSelf()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();

        builder.RegisterType<InvoiceRepository>()
            .As<IInvoiceRepository>()
            .InstancePerLifetimeScope();

        builder.RegisterType<CreateInvoiceCommandHandler>()
            .As<ICommandHandler<CreateInvoiceCommand, Guid>>()
            .InstancePerLifetimeScope();

        builder.RegisterType<GetInvoiceQueryHandler>()
            .As<IQueryHandler<GetInvoiceQuery, InvoiceDto?>>()
            .InstancePerLifetimeScope();
    }
}
