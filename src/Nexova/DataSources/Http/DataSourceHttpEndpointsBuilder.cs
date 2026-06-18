using Microsoft.AspNetCore.Mvc;
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
        api.MapPost("/", Create);
        api.MapPost("/upload", UploadFile)
            .DisableAntiforgery();
        api.MapPut("/{id:guid}", Update)
            .WithValidation<DataSourceRequest>();
        api.MapDelete("/{id:guid}", Delete);

        api.MapPost("/{id:guid}/files", AddFile);

        api.MapGet("/{id:guid}/tables", ListTables);
        api.MapGet("/{id:guid}/tables/{table}/columns", ListColumns);
        api.MapPost("/{id:guid}/test-connection", TestConnection);

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
        CreateDataSourceRequest request,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.Match<IResult>(
            created => Results.Created($"/api/datasources/{created.Id}", created),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> UploadFile(
        IFormFile? file,
        DataSourceService service,
        IStorageService storage,
        [FromForm] string? storageDirectory,
        CancellationToken cancellationToken)
    {
        var result = await service.UploadFileAsync(file, storageDirectory, storage, cancellationToken);
        return result.Match<IResult>(
            uploaded => Results.Ok(uploaded),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> AddFile(
        Guid id,
        DataSourceFileAssetRequest request,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.AddFileAsync(id, request, cancellationToken);
        return result.Match<IResult>(
            dataSource => Results.Ok(dataSource),
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

    private static async Task<IResult> ListTables(
        Guid id,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ListTablesAsync(id, cancellationToken);
        return result.Match<IResult>(
            tables => Results.Ok(tables),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> ListColumns(
        Guid id,
        string table,
        [FromQuery] string? schema,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ListColumnsAsync(id, table, schema, cancellationToken);
        return result.Match<IResult>(
            columns => Results.Ok(columns),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> TestConnection(
        Guid id,
        DataSourceService service,
        CancellationToken cancellationToken)
    {
        var result = await service.TestConnectionAsync(id, cancellationToken);
        return result.Match<IResult>(
            outcome => Results.Ok(outcome),
            errors => errors.ToProblem());
    }
}
