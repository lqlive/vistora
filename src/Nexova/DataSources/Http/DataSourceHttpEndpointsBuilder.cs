using Nexova.Core.Storage;
using Nexova.DataSources.Models;
using Nexova.Extensions;

namespace Nexova.DataSources.Http;

public static class DataSourceHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapDataSourceApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/datasources");
        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create)
            .WithValidation<DataSourceRequest>();
        api.MapPut("/{id:guid}", Update)
            .WithValidation<DataSourceRequest>();
        api.MapDelete("/{id:guid}", Delete);

        return api;
    }

    private static async Task<IResult> List(
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var dataSources = await service.ListAsync(cancellationToken);
        return Results.Ok(dataSources);
    }

    private static async Task<IResult> Get(
        Guid id,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.Match<IResult>(
            dataSource => Results.Ok(dataSource),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Create(
        DataSourceRequest request,
        DataSourceService service,
        IStorageService storage,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, storage, cancellationToken);
        return result.Match(
            created => Results.Created($"/api/datasources/{created.Id}", created),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Update(
        Guid id,
        DataSourceRequest request,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.Match(
            updated => Results.Ok(updated),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Delete(
        Guid id,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.Match(
            _ => Results.NoContent(),
            errors => errors.ToProblem());
    }
}
