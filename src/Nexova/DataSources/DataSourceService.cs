using ErrorOr;
using Microsoft.AspNetCore.Http;
using Nexova.Core.Entities;
using Nexova.Core.Storage;
using Nexova.Core.Stores;
using Nexova.DataSources.Errors;
using Nexova.DataSources.Models;

namespace Nexova.DataSources;

public sealed class DataSourceService(IDataSourceStore dataSourceStore)
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
            dataSource.FileAssets.Add(new DataSourceFileAsset
            {
                Id = Guid.NewGuid(),
                DataSourceId = dataSource.Id,
                DataSource = dataSource,
                FileName = asset.FileName,
                StoragePath = asset.StoragePath,
                ContentType = asset.ContentType,
                Size = asset.Size,
                HasHeader = asset.HasHeader,
                Delimiter = asset.Delimiter,
                Sheet = asset.Sheet
            });
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

    private static string BuildStoragePath(string? storageDirectory, string fileName)
    {
        var uniqueName = $"{Guid.NewGuid():N}-{fileName}";
        var directory = storageDirectory?.Trim().Trim('/');

        return string.IsNullOrWhiteSpace(directory)
            ? $"uploads/{uniqueName}"
            : $"{directory}/{uniqueName}";
    }

    private static bool TryParseType(string type, out DataSourceType parsed) =>
        Enum.TryParse(type.Trim(), ignoreCase: true, out parsed)
        && Enum.IsDefined(parsed);

}
