using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Invoicing.Domain.Invoice;

public interface IInvoiceRepository : IRepository<Invoice, Guid>
{
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
}
