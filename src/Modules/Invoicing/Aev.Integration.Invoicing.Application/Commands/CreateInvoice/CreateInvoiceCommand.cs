using Aev.Integration.BuildingBlocks.Application.CQRS;

namespace Aev.Integration.Invoicing.Application.Commands.CreateInvoice;

public record CreateInvoiceCommand(
    string InvoiceNumber,
    string DeliveryNoteNumber,
    decimal Amount) : ICommand<Guid>;
