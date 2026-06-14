using Nexova.Core.Entities;

namespace Nexova.Connectors.Abstractions;

public interface IQueryExecutor
{
    Task<QueryResult> ExecuteAsync(
        string sql,
        IReadOnlyCollection<DataSource> dataSources,
        CancellationToken cancellationToken = default);
}
