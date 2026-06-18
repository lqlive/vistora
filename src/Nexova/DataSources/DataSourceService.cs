using ErrorOr;
using Microsoft.AspNetCore.Http;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;
using Nexova.Core.Storage;
using Nexova.Core.Stores;
using Nexova.DataSources.Errors;
using Nexova.DataSources.Models;

namespace Nexova.DataSources;

public sealed class DataSourceService(IDataSourceStore dataSourceStore, IQueryExecutor queryExecutor)
{
    public async Task<IEnumerable<DataSourceResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var dataSources = await dataSourceStore.ListAsync(cancellationToken);
        return dataSources.Select(dataSource => dataSource.ToResponse());
    }

    public async Task<ErrorOr<DataSourceResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        return dataSource.ToResponse();
    }

    public async Task<ErrorOr<DataSourceResponse>> CreateAsync(
        CreateDataSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return DataSourceErrors.NameRequired;
        }

        if (!TryParseType(request.Type, out var type))
        {
            return DataSourceErrors.TypeInvalid;
        }

        var dataSource = new DataSource
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Type = type,
            Configuration = request.Configuration ?? new DataSourceConfiguration()
        };

        foreach (var asset in request.Files)
        {
            dataSource.FileAssets.Add(CreateFileAsset(dataSource, asset));
        }

        var created = await dataSourceStore.CreateAsync(dataSource, cancellationToken);
        if (!created)
        {
            return DataSourceErrors.NameAlreadyExists;
        }

        return dataSource.ToResponse();
    }

    public async Task<ErrorOr<DataSourceResponse>> UpdateAsync(
        Guid id,
        DataSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        if (!TryParseType(request.Type, out var type))
        {
            return DataSourceErrors.TypeInvalid;
        }

        dataSource.Name = request.Name.Trim();
        dataSource.Type = type;
        dataSource.Configuration = request.Configuration ?? new DataSourceConfiguration();

        var updated = await dataSourceStore.UpdateAsync(dataSource, cancellationToken);
        if (!updated)
        {
            return DataSourceErrors.NameAlreadyExists;
        }

        return dataSource.ToResponse();
    }

    public async Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await dataSourceStore.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return DataSourceErrors.NotFound;
        }

        return Result.Success;
    }

    public async Task<ErrorOr<FileUploadResponse>> UploadFileAsync(
        IFormFile? file,
        string? storageDirectory,
        IStorageService storage,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            return DataSourceErrors.FileRequired;
        }

        if (file.Length == 0)
        {
            return DataSourceErrors.FileEmpty;
        }

        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return DataSourceErrors.FileRequired;
        }

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        var storagePath = BuildStoragePath(storageDirectory, fileName);

        await using var content = file.OpenReadStream();
        var result = await storage.PutAsync(storagePath, content, contentType, cancellationToken);
        if (result == StoragePutResult.Conflict)
        {
            return DataSourceErrors.FileAlreadyExists;
        }

        return new FileUploadResponse(
            fileName,
            contentType,
            file.Length,
            storagePath,
            storagePath);
    }

    public async Task<ErrorOr<DataSourceResponse>> AddFileAsync(
        Guid id,
        DataSourceFileAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.StoragePath))
        {
            return DataSourceErrors.FileRequired;
        }

        var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        dataSource.FileAssets.Add(CreateFileAsset(dataSource, request));

        var updated = await dataSourceStore.UpdateAsync(dataSource, cancellationToken);
        if (!updated)
        {
            return DataSourceErrors.NameAlreadyExists;
        }

        return dataSource.ToResponse();
    }

    public async Task<ErrorOr<IReadOnlyList<TableInfo>>> ListTablesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        var tables = await queryExecutor.ListTablesAsync(dataSource, cancellationToken);
        return ErrorOrFactory.From(tables);
    }

    public async Task<ErrorOr<IReadOnlyList<ColumnInfo>>> ListColumnsAsync(
        Guid id,
        string table,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        var columns = await queryExecutor.ListColumnsAsync(dataSource, table, schema, cancellationToken);
        return ErrorOrFactory.From(columns);
    }

    public async Task<ErrorOr<ConnectionTestResult>> TestConnectionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        return await queryExecutor.TestConnectionAsync(dataSource, cancellationToken);
    }

    private static DataSourceFileAsset CreateFileAsset(DataSource dataSource, DataSourceFileAssetRequest request) =>
        new()
        {
            Id = Guid.NewGuid(),
            DataSourceId = dataSource.Id,
            DataSource = dataSource,
            FileName = request.FileName,
            StoragePath = request.StoragePath,
            ContentType = request.ContentType,
            Size = request.Size,
            HasHeader = request.HasHeader,
            Delimiter = request.Delimiter,
            Sheet = request.Sheet
        };

    private static string BuildStoragePath(string? storageDirectory, string fileName)
    {
        var uniqueName = $"{Guid.NewGuid():N}-{fileName}";
        var directory = storageDirectory?.Trim().Trim('/');

        return string.IsNullOrWhiteSpace(directory)
            ? $"uploads/{uniqueName}"
            : $"{directory}/{uniqueName}";
    }

    private static bool TryParseType(string type, out DataSourceType parsed)
    {
        switch (type.Trim().ToLowerInvariant())
        {
            case "file":
            case "files":
                parsed = DataSourceType.Files;
                return true;
            case "mysql":
                parsed = DataSourceType.MySql;
                return true;
            case "postgres":
            case "postgresql":
                parsed = DataSourceType.PostgreSql;
                return true;
            case "sqlite":
                parsed = DataSourceType.Sqlite;
                return true;
            case "clickhouse":
                parsed = DataSourceType.ClickHouse;
                return true;
            case "mongo":
            case "mongodb":
                parsed = DataSourceType.MongoDb;
                return true;
        }

        return Enum.TryParse(type.Trim(), ignoreCase: true, out parsed)
            && Enum.IsDefined(parsed);
    }

}
