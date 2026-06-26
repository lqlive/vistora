using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
            ParseColumns(dataset.ColumnsJson),
            dataset.CreatedAt,
            dataset.UpdatedAt);

    private static IReadOnlyList<DatasetColumnResponse> ParseColumns(string? columnsJson)
    {
        if (string.IsNullOrWhiteSpace(columnsJson))
        {
            return [];
        }

        try
        {
            var columns = JsonSerializer.Deserialize<List<DatasetColumnInfo>>(columnsJson);
            if (columns is null)
            {
                return [];
            }

            return columns
                .Select(column => new DatasetColumnResponse(
                    DeterministicId(column.Name),
                    column.Name,
                    column.Type,
                    column.Nullable,
                    column.Precision,
                    column.Scale,
                    column.Ordinal))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Guid DeterministicId(string name) =>
        new(MD5.HashData(Encoding.UTF8.GetBytes(name)));
}
