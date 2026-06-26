using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Nexova.Extensions;
using Nexova.Queries.Models;
using Nexova.Users.Authentication;

namespace Nexova.Users.Http;

public static class QueryDocumentHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapQueryDocumentApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/user/query-documents");
        api.MapGet("/", List);
        api.MapGet("/{id:guid}", Get);
        api.MapPost("/", Create);
        api.MapPut("/{id:guid}", Update);
        api.MapDelete("/{id:guid}", Delete);

        return api;
    }

    private static async Task<IResult> List(
        ClaimsPrincipal principal,
        QueryDocumentService service,
        [FromQuery] QueryDocumentScope scope = QueryDocumentScope.Accessible,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var documents = await service.ListAsync(userId, scope, cancellationToken);
        return Results.Ok(documents);
    }

    private static async Task<IResult> Get(
        Guid id,
        ClaimsPrincipal principal,
        QueryDocumentService service,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.GetAsync(userId, id, cancellationToken);
        return result.Match<IResult>(
            document => Results.Ok(document),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Create(
        QueryDocumentRequest request,
        ClaimsPrincipal principal,
        QueryDocumentService service,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.CreateAsync(userId, request, cancellationToken);
        return result.Match<IResult>(
            document => Results.Created($"/api/user/query-documents/{document.Id}", document),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Update(
        Guid id,
        QueryDocumentRequest request,
        ClaimsPrincipal principal,
        QueryDocumentService service,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.UpdateAsync(userId, id, request, cancellationToken);
        return result.Match<IResult>(
            document => Results.Ok(document),
            errors => errors.ToProblem());
    }

    private static async Task<IResult> Delete(
        Guid id,
        ClaimsPrincipal principal,
        QueryDocumentService service,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.DeleteAsync(userId, id, cancellationToken);
        return result.Match<IResult>(
            _ => Results.NoContent(),
            errors => errors.ToProblem());
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
    {
        var value = principal.FindFirstValue(AuthenticationConstants.UserIdClaimType);
        return Guid.TryParse(value, out userId);
    }
}
