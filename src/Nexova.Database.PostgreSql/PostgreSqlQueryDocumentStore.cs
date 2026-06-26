using Nexova.Core.Entities;
using Nexova.Core.Stores;

namespace Nexova.Database.PostgreSql;

public class PostgreSqlQueryDocumentStore : IQueryDocumentStore
{
    public Task<bool> CreateAsync(QueryDocument document, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<QueryDocument?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<QueryDocument>> ListAccessibleAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<QueryDocument>> ListByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<QueryDocument>> ListSharedAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(QueryDocument document, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
