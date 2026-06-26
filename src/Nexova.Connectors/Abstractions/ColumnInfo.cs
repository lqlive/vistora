namespace Nexova.Connectors.Abstractions;

public sealed record ColumnInfo(
    string Name,
    string Type,
    bool Nullable,
    int? Precision,
    int? Scale);
