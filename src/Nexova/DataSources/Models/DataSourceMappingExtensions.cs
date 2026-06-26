using Nexova.Core.Entities;

namespace Nexova.DataSources.Models;

public static class DataSourceMappingExtensions
{
    public static DataSourceResponse ToResponse(this DataSource dataSource) =>
        new(
            dataSource.Id,
            dataSource.Name,
            dataSource.Type.ToString(),
            dataSource.Configuration,
            dataSource.FileAssets.Select(ToResponse).ToList(),
            dataSource.CreatedAt,
            dataSource.UpdatedAt);

    public static DataSourceFileResponse ToResponse(this DataSourceFileAsset asset) =>
        new(
            asset.Id,
            asset.DataSourceId,
            asset.FileName,
            asset.StoragePath,
            asset.ContentType,
            asset.Size,
            asset.HasHeader,
            asset.Delimiter,
            asset.Sheet,
            asset.CreatedAt);
}
