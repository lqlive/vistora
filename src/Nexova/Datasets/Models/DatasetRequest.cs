namespace Nexova.Datasets.Models;

public sealed record DatasetRequest(
    string Name,
    string Sql,
    string? Description);
