using System.Collections.Concurrent;
using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public class InMemoryQueryDocumentStore : IQueryDocumentStore
{
    private static readonly ConcurrentDictionary<Guid, QueryDocument> documents = [];

    public Task<bool> CreateAsync(QueryDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (documents.Values.Any(existing => HasSameOwnerAndName(existing, document)))
        {
            return Task.FromResult(false);
        }

        document.CreatedAt = DateTimeOffset.UtcNow;
        document.UpdatedAt = document.CreatedAt;
        return Task.FromResult(documents.TryAdd(document.Id, document));
    }

    public Task<QueryDocument?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        documents.TryGetValue(id, out var document);
        return Task.FromResult(document);
    }

    public Task<IReadOnlyList<QueryDocument>> ListAccessibleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<QueryDocument> items = documents.Values
            .Where(document => document.UserId == userId || document.IsShared)
            .OrderBy(document => document.IsShared && document.UserId != userId)
            .ThenBy(document => document.Name)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<QueryDocument>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<QueryDocument> items = documents.Values
            .Where(document => document.UserId == userId)
            .OrderBy(document => document.Name)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<QueryDocument>> ListSharedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<QueryDocument> items = documents.Values
            .Where(document => document.IsShared)
            .OrderBy(document => document.Name)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<bool> UpdateAsync(QueryDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!documents.ContainsKey(document.Id))
        {
            return Task.FromResult(false);
        }

        if (documents.Values.Any(existing => existing.Id != document.Id && HasSameOwnerAndName(existing, document)))
        {
            return Task.FromResult(false);
        }

        document.UpdatedAt = DateTimeOffset.UtcNow;
        documents[document.Id] = document;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(documents.TryRemove(id, out _));
    }

    private static bool HasSameOwnerAndName(QueryDocument left, QueryDocument right) =>
        left.UserId == right.UserId
        && string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
}
