using System.Text.Json;
using ErrorOr;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;
using Nexova.Core.Stores;
using Nexova.Datasets.Errors;
using Nexova.Datasets.Models;

namespace Nexova.Datasets;

public sealed class DatasetService(
    IDatasetStore datasetStore,
    IDataSourceStore dataSourceStore,
    IQueryExecutor queryExecutor)
{
    public async Task<IReadOnlyList<DatasetResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var datasets = await datasetStore.ListAsync(cancellationToken);
        return datasets.Select(dataset => dataset.ToResponse()).ToList();
    }

    public async Task<ErrorOr<DatasetResponse>> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dataset = await datasetStore.GetAsync(id, cancellationToken);
        if (dataset is null)
        {
            return DatasetErrors.NotFound;
        }

        return dataset.ToResponse();
    }

    public async Task<ErrorOr<DatasetResponse>> CreateAsync(
        DatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return DatasetErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return DatasetErrors.SqlRequired;
        }

        var dataset = new Dataset
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Sql = request.Sql.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        dataset.ColumnsJson = await InferColumnsJsonAsync(dataset.Sql, cancellationToken);

        var created = await datasetStore.CreateAsync(dataset, cancellationToken);
        if (!created)
        {
            return DatasetErrors.NameAlreadyExists;
        }

        return dataset.ToResponse();
    }

    public async Task<ErrorOr<DatasetResponse>> UpdateAsync(
        Guid id,
        DatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return DatasetErrors.NameRequired;
        }

        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return DatasetErrors.SqlRequired;
        }

        var dataset = await datasetStore.GetAsync(id, cancellationToken);
        if (dataset is null)
        {
            return DatasetErrors.NotFound;
        }

        dataset.Name = request.Name.Trim();
        dataset.Sql = request.Sql.Trim();
        dataset.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        dataset.ColumnsJson = await InferColumnsJsonAsync(dataset.Sql, cancellationToken);

        var updated = await datasetStore.UpdateAsync(dataset, cancellationToken);
        if (!updated)
        {
            return DatasetErrors.NameAlreadyExists;
        }

        return dataset.ToResponse();
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await datasetStore.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return DatasetErrors.NotFound;
        }

        return Result.Deleted;
    }

    private async Task<string?> InferColumnsJsonAsync(string sql, CancellationToken cancellationToken)
    {
        try
        {
            var dataSources = await dataSourceStore.ListAsync(cancellationToken);
            var columns = await queryExecutor.DescribeAsync(sql, dataSources, cancellationToken);
            if (columns.Count == 0)
            {
                return null;
            }

            var infos = columns
                .Select((column, ordinal) => new DatasetColumnInfo(
                    column.Name,
                    column.Type,
                    column.Nullable,
                    column.Precision,
                    column.Scale,
                    ordinal))
                .ToList();

            return JsonSerializer.Serialize(infos);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Best-effort schema inference: a bad SQL or unavailable data source
            // should not block saving the dataset.
            return null;
        }
    }
}
