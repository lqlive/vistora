using Nexova.Core.Entities;

namespace Nexova.Datasets.Models;

public static class DatasetMappingExtensions
{
    public static DatasetResponse ToResponse(this Dataset dataset) =>
        new(
            dataset.Id,
            dataset.Name,
            dataset.Sql,
            dataset.Description,
            [],
            [],
            dataset.CreatedAt,
            dataset.UpdatedAt);
}
