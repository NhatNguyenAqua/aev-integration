using Aev.Integration.BuildingBlocks.Application.CQRS;
using Aev.Integration.DataSync.Application.Commands.TriggerSync;
using Aev.Integration.DataSync.Application.Queries.GetSyncJobs;

namespace Aev.Integration.API.Modules;

public static class DataSyncEndpoints
{
    public static IEndpointRouteBuilder MapDataSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/data-sync").WithTags("DataSync");

        group.MapPost("/trigger", async (
            TriggerSyncRequest request,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            var jobId = await commandDispatcher.DispatchAsync<TriggerSyncCommand, Guid>(
                new TriggerSyncCommand(request.SystemSource), cancellationToken);
            return Results.Created($"/api/data-sync/{jobId}", new { id = jobId });
        }).RequireAuthorization();

        group.MapGet("/jobs", async (
            string? systemSource,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken) =>
        {
            var jobs = await queryDispatcher.DispatchAsync<GetSyncJobsQuery, IReadOnlyList<SyncJobDto>>(
                new GetSyncJobsQuery(systemSource), cancellationToken);
            return Results.Ok(jobs);
        }).RequireAuthorization();

        return app;
    }
}

public record TriggerSyncRequest(string SystemSource);
