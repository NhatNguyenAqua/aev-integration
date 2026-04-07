using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.Invoicing.Application.Queries.GetInvoice;

public record GetInvoiceQuery(Guid InvoiceId) : IQuery<InvoiceDto?>;
