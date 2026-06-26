namespace Nexova.Queries.Models;

public sealed record ExplainRequest(
    Guid DataSourceId,
    string Sql,
    int? Limit,
    int? TimeoutMs);
