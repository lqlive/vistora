using ErrorOr;
using Nexova.Core.Entities;
using Nexova.Core.Stores;
using Nexova.Queries.Errors;
using Nexova.Queries.Models;

namespace Nexova.Users;

public sealed class QueryDocumentService(IQueryDocumentStore queryDocumentStore)
{
    public async Task<IReadOnlyList<QueryDocumentResponse>> ListAsync(
        Guid userId,
        QueryDocumentScope scope = QueryDocumentScope.Accessible,
        CancellationToken cancellationToken = default)
    {
        var documents = scope switch
        {
            QueryDocumentScope.My => await queryDocumentStore.ListByUserAsync(userId, cancellationToken),
            QueryDocumentScope.Shared => await queryDocumentStore.ListSharedAsync(cancellationToken),
            _ => await queryDocumentStore.ListAccessibleAsync(userId, cancellationToken)
        };

        return documents
            .Select(document => document.ToResponse(userId))
            .ToList();
    }

    public async Task<ErrorOr<QueryDocumentResponse>> GetAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var document = await queryDocumentStore.GetAsync(id, cancellationToken);
        if (document is null)
        {
            return QueryDocumentErrors.NotFound;
        }

        if (!CanRead(document, userId))
        {
            return QueryDocumentErrors.AccessDenied;
        }

        return document.ToResponse(userId);
    }

    public async Task<ErrorOr<QueryDocumentResponse>> CreateAsync(
        Guid userId,
        QueryDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return QueryDocumentErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return QueryDocumentErrors.SqlRequired;
        }

        var document = new QueryDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Sql = request.Sql.Trim(),
            IsShared = request.IsShared
        };

        var created = await queryDocumentStore.CreateAsync(document, cancellationToken);
        if (!created)
        {
            return QueryDocumentErrors.NameAlreadyExists;
        }

        return document.ToResponse(userId);
    }

    public async Task<ErrorOr<QueryDocumentResponse>> UpdateAsync(
        Guid userId,
        Guid id,
        QueryDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return QueryDocumentErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return QueryDocumentErrors.SqlRequired;
        }

        var document = await queryDocumentStore.GetAsync(id, cancellationToken);
        if (document is null)
        {
            return QueryDocumentErrors.NotFound;
        }

        if (document.UserId != userId)
        {
            return QueryDocumentErrors.AccessDenied;
        }

        document.Name = request.Name.Trim();
        document.Sql = request.Sql.Trim();
        document.IsShared = request.IsShared;

        var updated = await queryDocumentStore.UpdateAsync(document, cancellationToken);
        if (!updated)
        {
            return QueryDocumentErrors.NameAlreadyExists;
        }

        return document.ToResponse(userId);
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var document = await queryDocumentStore.GetAsync(id, cancellationToken);
        if (document is null)
        {
            return QueryDocumentErrors.NotFound;
        }

        if (document.UserId != userId)
        {
            return QueryDocumentErrors.AccessDenied;
        }

        await queryDocumentStore.DeleteAsync(id, cancellationToken);
        return Result.Deleted;
    }

    private static bool CanRead(QueryDocument document, Guid userId) =>
        document.UserId == userId || document.IsShared;
}

public enum QueryDocumentScope
{
    Accessible,
    My,
    Shared
}
