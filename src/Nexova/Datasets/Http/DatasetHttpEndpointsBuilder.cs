using Nexova.Datasets.Models;
using Nexova.Extensions;

namespace Nexova.Datasets.Http;

public static class DatasetHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapDatasetApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/datasets");
        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create);
        api.MapPut("/{id:guid}", Update);
        api.MapDelete("/{id:guid}", Delete);

        return api;
    }

    private static async Task<IResult> List(
        DatasetService service,
        CancellationToken cancellationToken)
    {
        var datasets = await service.ListAsync(cancellationToken);
        return Results.Ok(datasets);
    }

    private static async Task<IResult> Get(
        Guid id,
        DatasetService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.Match<IResult>(
            dataset => Results.Ok(dataset),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Create(
        DatasetRequest request,
        DatasetService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Match<IResult>(
            dataset => Results.Created($"/api/datasets/{dataset.Id}", dataset),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Update(
        Guid id,
        DatasetRequest request,
        DatasetService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.Match<IResult>(
            dataset => Results.Ok(dataset),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Delete(
        Guid id,
        DatasetService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.Match<IResult>(
            _ => Results.NoContent(),
            errors => errors.ToProblem());
    }
}
