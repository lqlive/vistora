namespace Nexova.Query.Models;

public sealed record ExplainRequest(
    Guid DataSourceId,
    string Sql,
    int? Limit,
    int? TimeoutMs);
