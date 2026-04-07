using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.Invoicing.Application.Commands.CreateInvoice;
using Aev.Integration.Invoicing.Application.Queries.GetInvoice;

namespace Aev.Integration.API.Modules;

public static class InvoicingEndpoints
{
    public static IEndpointRouteBuilder MapInvoicingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoicing").WithTags("Invoicing");

        group.MapPost("/invoices", async (
            CreateInvoiceRequest request,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            var invoiceId = await commandDispatcher.DispatchAsync<CreateInvoiceCommand, Guid>(
                new CreateInvoiceCommand(request.InvoiceNumber, request.DeliveryNoteNumber, request.Amount),
                cancellationToken);
            return Results.Created($"/api/invoicing/invoices/{invoiceId}", new { id = invoiceId });
        }).RequireAuthorization();

        group.MapGet("/invoices/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken) =>
        {
            var invoice = await queryDispatcher.DispatchAsync<GetInvoiceQuery, InvoiceDto?>(
                new GetInvoiceQuery(id), cancellationToken);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        }).RequireAuthorization();

        return app;
    }
}

public record CreateInvoiceRequest(string InvoiceNumber, string DeliveryNoteNumber, decimal Amount);
