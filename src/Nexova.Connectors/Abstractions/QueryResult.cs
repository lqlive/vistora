namespace Nexova.Connectors.Abstractions;

public sealed class QueryResult
{
    public IReadOnlyList<ColumnInfo> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<object?>> Rows { get; init; } = [];
}
