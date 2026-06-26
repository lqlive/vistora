using Nexova.Charts.Models;
using Nexova.Extensions;

namespace Nexova.Charts.Http;

public static class ChartHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapChartApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/charts");
        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create)
            .WithValidation<ChartRequest>();
        api.MapPut("/{id:guid}", Update)
            .WithValidation<ChartRequest>();
        api.MapDelete("/{id:guid}", Delete);

        return api;
    }

    private static async Task<IResult> List(
        ChartService service,
        CancellationToken cancellationToken)
    {
        var charts = await service.ListAsync(cancellationToken);
        return Results.Ok(charts);
    }

    private static async Task<IResult> Get(
        Guid id,
        ChartService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.Match<IResult>(
            chart => Results.Ok(chart),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Create(
        ChartRequest request,
        ChartService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Match<IResult>(
            chart => Results.Created($"/api/charts/{chart.Id}", chart),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Update(
        Guid id,
        ChartRequest request,
        ChartService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.Match<IResult>(
            chart => Results.Ok(chart),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Delete(
        Guid id,
        ChartService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.Match<IResult>(
            _ => Results.NoContent(),
            errors => errors.ToProblem());
    }
}
