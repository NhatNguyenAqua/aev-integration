using Aev.Integration.BuildingBlocks.Infrastructure.Persistence;
using Aev.Integration.Invoicing.Domain.Invoice;
using Microsoft.EntityFrameworkCore;

namespace Aev.Integration.Invoicing.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(InvoicingDbContext context)
    : BaseRepository<Invoice, Guid, InvoicingDbContext>(context), IInvoiceRepository
{
    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
        => await Context.Invoices.FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber, cancellationToken);
}
