using Nexova.Connectors.Abstractions;

namespace Nexova.Query.Models;

public sealed record QueryResponse(
    IReadOnlyList<ColumnInfo> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    int RowCount,
    long DurationMs);
