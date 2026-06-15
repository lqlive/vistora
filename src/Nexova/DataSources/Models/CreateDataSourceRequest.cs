using Nexova.Core.Entities;

namespace Nexova.DataSources.Models;

public sealed record CreateDataSourceRequest(
    string Name,
    string Type,
    DataSourceConfiguration? Configuration,
    IReadOnlyList<DataSourceFileAssetRequest> Files);

public sealed record DataSourceFileAssetRequest(
    string FileName,
    string StoragePath,
    string ContentType,
    long Size,
    bool? HasHeader,
    string? Delimiter,
    string? Sheet);
