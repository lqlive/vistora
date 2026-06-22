using Nexova.Core.Configuration;
using Nexova.Core.Entities;
using Nexova.Core.Storage;
using Nexova.Core.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nexova.Core.Management;

public static class INexovaBuilderExtensions
{
    public static INexovaBuilder AddInMemoryStore(this INexovaBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;
        services.AddKeyedScoped<IDataSourceStore, InMemoryDataSourceStore>(InMemoryStoreOptions.Name);
        services.AddKeyedScoped<IDatasetStore, InMemoryDatasetStore>(InMemoryStoreOptions.Name);
        services.AddKeyedScoped<IChartStore, InMemoryChartStore>(InMemoryStoreOptions.Name);
        services.AddKeyedScoped<IUserStore, InMemoryUserStore>(InMemoryStoreOptions.Name);
        services.AddKeyedScoped<IQueryDocumentStore, InMemoryQueryDocumentStore>(InMemoryStoreOptions.Name);

        return builder;
    }

    public static INexovaBuilder AddFileStorage(this INexovaBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;
        services.AddKeyedTransient<IStorageService, FileStorageService>(FileSystemStorageOptions.Name);

        return builder;
    }

    public static INexovaBuilder AddDbContextProvider<TContext>(
        this INexovaBuilder builder,
        string databaseType,
        Action<IServiceProvider, DbContextOptionsBuilder> configureContext)
        where TContext : DbContext, IContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(databaseType);
        ArgumentNullException.ThrowIfNull(configureContext);

        var services = builder.Services;
        services.AddDbContext<TContext>(configureContext);
        services.AddKeyedScoped<IContext>(databaseType, (provider, _) => provider.GetRequiredService<TContext>());

        return builder;
    }

    public static INexovaBuilder AddDbContextProvider<TContext, TDataSourceStore, TDatasetStore, TChartStore>(
        this INexovaBuilder builder,
        string databaseType,
        Action<IServiceProvider, DbContextOptionsBuilder> configureContext)
        where TContext : DbContext, IContext
        where TDataSourceStore : class, IDataSourceStore
        where TDatasetStore : class, IDatasetStore
        where TChartStore : class, IChartStore
    {
        builder.AddDbContextProvider<TContext>(databaseType, configureContext);

        var services = builder.Services;
        services.AddKeyedScoped<IDataSourceStore, TDataSourceStore>(databaseType);
        services.AddKeyedScoped<IDatasetStore, TDatasetStore>(databaseType);
        services.AddKeyedScoped<IChartStore, TChartStore>(databaseType);

        return builder;
    }
}
