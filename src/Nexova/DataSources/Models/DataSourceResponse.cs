using Nexova.Core.Entities;

namespace Nexova.DataSources.Models;

public sealed record DataSourceResponse(
    Guid Id,
    string Name,
    string Type,
    DataSourceConfiguration Configuration,
    IReadOnlyList<DataSourceFileResponse> Files,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

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
