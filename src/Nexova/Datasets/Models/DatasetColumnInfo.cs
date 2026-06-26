namespace Nexova.Datasets.Models;

public sealed record DatasetColumnInfo(
    string Name,
    string Type,
    bool Nullable,
    int? Precision,
    int? Scale,
    int Ordinal);
