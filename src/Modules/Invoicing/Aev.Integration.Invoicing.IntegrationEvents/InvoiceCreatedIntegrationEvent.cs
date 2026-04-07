namespace Aev.Integration.Invoicing.IntegrationEvents;

public record InvoiceCreatedIntegrationEvent(
    Guid Id,
    string InvoiceNumber,
    decimal Amount,
    DateTime OccurredOn);
