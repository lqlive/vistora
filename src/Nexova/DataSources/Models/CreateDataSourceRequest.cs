using Nexova.Core.Entities;

namespace Nexova.DataSources.Models;

public sealed record CreateDataSourceRequest(
    string Name,
    string Type,
    DataSourceConfiguration? Configuration,
    IReadOnlyList<DataSourceFileAssetRequest> Files);
