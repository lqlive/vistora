namespace Nexova.DataSources.Models;

public sealed record DataSourceFileAssetRequest(
    string FileName,
    string StoragePath,
    string ContentType,
    long Size,
    bool? HasHeader,
    string? Delimiter,
    string? Sheet);
