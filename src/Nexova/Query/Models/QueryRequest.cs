namespace Nexova.Query.Models;

public sealed record QueryRequest(
    IReadOnlyList<Guid> DataSourceIds,
    string Sql,
    int? Limit,
    int? TimeoutMs);
