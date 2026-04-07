using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.ServiceRequest.Application.Commands.CreateServiceRequest;
using Aev.Integration.ServiceRequest.Application.Queries.GetServiceRequests;
using Aev.Integration.ServiceRequest.Domain.ServiceRequest;

namespace Aev.Integration.API.Modules;

public static class ServiceRequestEndpoints
{
    public static IEndpointRouteBuilder MapServiceRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/service-requests").WithTags("ServiceRequest");

        group.MapPost("/", async (
            CreateServiceRequestRequest request,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            var requestId = await commandDispatcher.DispatchAsync<CreateServiceRequestCommand, Guid>(
                new CreateServiceRequestCommand(
                    request.CustomerCode,
                    request.CustomerType,
                    request.RequestType,
                    request.Description),
                cancellationToken);
            return Results.Created($"/api/service-requests/{requestId}", new { id = requestId });
        }).RequireAuthorization();

        group.MapGet("/{customerCode}", async (
            string customerCode,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken) =>
        {
            var requests = await queryDispatcher.DispatchAsync<GetServiceRequestsQuery, IReadOnlyList<ServiceRequestDto>>(
                new GetServiceRequestsQuery(customerCode), cancellationToken);
            return Results.Ok(requests);
        }).RequireAuthorization();

        return app;
    }
}

public record CreateServiceRequestRequest(
    string CustomerCode,
    CustomerType CustomerType,
    string RequestType,
    string Description);
