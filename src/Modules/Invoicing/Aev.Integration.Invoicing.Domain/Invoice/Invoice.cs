using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Invoicing.Domain.Invoice;

public class Invoice : AggregateRoot<Guid>
{
    public string InvoiceNumber { get; private set; } = default!;
    public string DeliveryNoteNumber { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string BarcodeValue { get; private set; } = default!;
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedAt { get; private set; }

    private Invoice() { }

    public static Invoice Create(string invoiceNumber, string deliveryNoteNumber, decimal amount)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            DeliveryNoteNumber = deliveryNoteNumber,
            Amount = amount,
            BarcodeValue = GenerateBarcode(invoiceNumber),
            Status = InvoiceStatus.Draft,
            IssuedAt = DateTime.UtcNow
        };

        invoice.AddDomainEvent(new InvoiceCreatedDomainEvent(invoice.Id, invoiceNumber, amount));
        return invoice;
    }

    private static string GenerateBarcode(string invoiceNumber) =>
        $"INV-{invoiceNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";

    public void Approve()
    {
        Status = InvoiceStatus.Approved;
        IncrementVersion();
    }
}
