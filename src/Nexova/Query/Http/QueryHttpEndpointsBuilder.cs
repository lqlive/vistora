using Nexova.Extensions;
using Nexova.Query.Models;

namespace Nexova.Query.Http;

public static class QueryHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapQueryApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/query");
        api.MapPost("/", Execute);
        api.MapPost("/explain", Explain);

        return api;
    }

    private static async Task<IResult> Execute(
        QueryRequest request,
        QueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(request, cancellationToken);
        return result.Match<IResult>(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Explain(
        ExplainRequest request,
        QueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExplainAsync(request, cancellationToken);
        return result.Match<IResult>(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }
}
