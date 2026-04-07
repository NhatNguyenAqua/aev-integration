namespace Aev.Integration.Invoicing.Application.Queries.GetInvoice;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string DeliveryNoteNumber,
    decimal Amount,
    string BarcodeValue,
    string Status,
    DateTime IssuedAt);
