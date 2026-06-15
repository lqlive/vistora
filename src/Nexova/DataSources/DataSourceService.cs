using ErrorOr;
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
        DataSourceRequest request,
        IStorageService storage,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseType(request.Type, out var type))
        {
            return DataSourceErrors.TypeInvalid;
        }

        var dataSource = new DataSource
        {
            Name = request.Name.Trim(),
            Type = type,
            Configuration = request.Configuration ?? new DataSourceConfiguration()
        };

        await InitializeFileContainerAsync(dataSource, storage, cancellationToken);

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

    private static bool TryParseType(string type, out DataSourceType parsed) =>
        Enum.TryParse(type.Trim(), ignoreCase: true, out parsed)
        && Enum.IsDefined(parsed);

    private static Task InitializeFileContainerAsync(
        DataSource dataSource,
        IStorageService storage,
        CancellationToken cancellationToken)
    {
        if (dataSource.Type != DataSourceType.File)
        {
            return Task.CompletedTask;
        }

        // Object storage has no real directories, so the container is just a path
        // prefix derived from the data source id. Uploaded file assets live under it.
        if (dataSource.Id == Guid.Empty)
        {
            dataSource.Id = Guid.NewGuid();
        }

        dataSource.Configuration.StoragePath = $"datasources/{dataSource.Id:N}";
        return Task.CompletedTask;
    }
}
