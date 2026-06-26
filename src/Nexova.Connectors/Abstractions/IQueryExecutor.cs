using Nexova.Core.Entities;

namespace Nexova.Connectors.Abstractions;

public interface IQueryExecutor
{
    Task<QueryResult> ExecuteAsync(
        string sql,
        IReadOnlyCollection<DataSource> dataSources,
        int? maxRows = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ColumnInfo>> DescribeAsync(
        string sql,
        IReadOnlyCollection<DataSource> dataSources,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TableInfo>> ListTablesAsync(
        DataSource dataSource,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ColumnInfo>> ListColumnsAsync(
        DataSource dataSource,
        string table,
        string? schema = null,
        CancellationToken cancellationToken = default);

    Task<ConnectionTestResult> TestConnectionAsync(
        DataSource dataSource,
        CancellationToken cancellationToken = default);
}
