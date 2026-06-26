using Nexova.Dashboards.Models;
using Nexova.Extensions;

namespace Nexova.Dashboards.Http;

public static class DashboardHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapDashboardApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/dashboards");
        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create)
            .WithValidation<DashboardRequest>();
        api.MapPut("/{id:guid}", Update)
            .WithValidation<DashboardRequest>();
        api.MapDelete("/{id:guid}", Delete);

        return api;
    }

    private static async Task<IResult> List(
        DashboardService service,
        CancellationToken cancellationToken)
    {
        var dashboards = await service.ListAsync(cancellationToken);
        return Results.Ok(dashboards);
    }

    private static async Task<IResult> Get(
        Guid id,
        DashboardService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.Match<IResult>(
            dashboard => Results.Ok(dashboard),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Create(
        DashboardRequest request,
        DashboardService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Match<IResult>(
            dashboard => Results.Created($"/api/dashboards/{dashboard.Id}", dashboard),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Update(
        Guid id,
        DashboardRequest request,
        DashboardService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.Match<IResult>(
            dashboard => Results.Ok(dashboard),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Delete(
        Guid id,
        DashboardService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.Match<IResult>(
            _ => Results.NoContent(),
            errors => errors.ToProblem());
    }
}
