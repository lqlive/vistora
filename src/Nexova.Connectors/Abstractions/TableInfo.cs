namespace Nexova.Connectors.Abstractions;

public sealed record TableInfo(
    string? Schema,
    string Name,
    string Type);
