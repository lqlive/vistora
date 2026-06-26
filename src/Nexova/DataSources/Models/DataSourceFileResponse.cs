namespace Nexova.DataSources.Models;

public sealed record DataSourceFileResponse(
    Guid Id,
    Guid DataSourceId,
    string FileName,
    string StoragePath,
    string ContentType,
    long Size,
    bool? HasHeader,
    string? Delimiter,
    string? Sheet,
    DateTimeOffset CreatedAt);
