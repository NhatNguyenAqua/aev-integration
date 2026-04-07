using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.Invoicing.Domain.Invoice;

namespace Aev.Integration.Invoicing.Application.Queries.GetInvoice;

public sealed class GetInvoiceQueryHandler(IInvoiceRepository invoiceRepository)
    : IQueryHandler<GetInvoiceQuery, InvoiceDto?>
{
    public async Task<InvoiceDto?> HandleAsync(GetInvoiceQuery query, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository.GetByIdAsync(query.InvoiceId, cancellationToken);

        return invoice is null ? null : new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.DeliveryNoteNumber,
            invoice.Amount,
            invoice.BarcodeValue,
            invoice.Status.ToString(),
            invoice.IssuedAt);
    }
}
