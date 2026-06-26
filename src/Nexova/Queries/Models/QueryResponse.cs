using Nexova.Connectors.Abstractions;

namespace Nexova.Queries.Models;

public sealed record QueryResponse(
    IReadOnlyList<ColumnInfo> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    int RowCount,
    long DurationMs);
