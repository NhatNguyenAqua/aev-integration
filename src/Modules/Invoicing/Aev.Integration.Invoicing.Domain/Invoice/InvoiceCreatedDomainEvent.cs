using Aev.Integration.BuildingBlocks.Domain;

namespace Aev.Integration.Invoicing.Domain.Invoice;

public record InvoiceCreatedDomainEvent(Guid InvoiceId, string InvoiceNumber, decimal Amount) : DomainEvent;
