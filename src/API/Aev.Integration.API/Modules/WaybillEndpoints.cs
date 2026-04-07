using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.Waybill.Application.Commands.CreateWaybill;
using Aev.Integration.Waybill.Application.Queries.GetWaybill;

namespace Aev.Integration.API.Modules;

public static class WaybillEndpoints
{
    public static IEndpointRouteBuilder MapWaybillEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/waybills").WithTags("Waybill");

        group.MapPost("/", async (
            CreateWaybillRequest request,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            var waybillId = await commandDispatcher.DispatchAsync<CreateWaybillCommand, Guid>(
                new CreateWaybillCommand(request.WaybillNumber, request.SenderAddress, request.ReceiverAddress),
                cancellationToken);
            return Results.Created($"/api/waybills/{waybillId}", new { id = waybillId });
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken) =>
        {
            var waybill = await queryDispatcher.DispatchAsync<GetWaybillQuery, WaybillDto?>(
                new GetWaybillQuery(id), cancellationToken);
            return waybill is null ? Results.NotFound() : Results.Ok(waybill);
        }).RequireAuthorization();

        return app;
    }
}

public record CreateWaybillRequest(string WaybillNumber, string SenderAddress, string ReceiverAddress);
