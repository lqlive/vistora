using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public interface IQueryDocumentStore
{
    Task<bool> CreateAsync(QueryDocument document, CancellationToken cancellationToken);
    Task<QueryDocument?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<QueryDocument>> ListAccessibleAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<QueryDocument>> ListByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<QueryDocument>> ListSharedAsync(CancellationToken cancellationToken);
    Task<bool> UpdateAsync(QueryDocument document, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
