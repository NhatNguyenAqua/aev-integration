using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.BuildingBlocks.Domain;
using Aev.Integration.Invoicing.Domain.Invoice;

namespace Aev.Integration.Invoicing.Application.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateInvoiceCommand, Guid>
{
    public async Task<Guid> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = Invoice.Create(command.InvoiceNumber, command.DeliveryNoteNumber, command.Amount);

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }
}
