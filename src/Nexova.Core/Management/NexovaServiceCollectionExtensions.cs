using Nexova.Core.Configuration;
using Nexova.Core.Configuration.Validators;
using Nexova.Core.Entities;
using Nexova.Core.Storage;
using Nexova.Core.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Nexova.Core.Management;

public static class NexovaServiceCollectionExtensions
{
    public static INexovaBuilder AddNexovaCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new NexovaBuilder(services);

        services.AddProviderResolver<IDataSourceStore, DatabaseOptions>(options => options.Type);
        services.AddProviderResolver<IDatasetStore, DatabaseOptions>(options => options.Type);
        services.AddProviderResolver<IUserStore, DatabaseOptions>(options => options.Type);
        services.AddProviderResolver<IQueryDocumentStore, DatabaseOptions>(options => options.Type);
        services.AddProviderResolver<IContext, DatabaseOptions>(options => options.Type);
        services.AddProviderResolver<IStorageService, StorageOptions>(options => options.Type);

        services.AddNexovaOptions();
        return builder;
    }

    private static void AddProviderResolver<TService, TOptions>(
        this IServiceCollection services,
        Func<TOptions, string?> keySelector)
        where TService : class
        where TOptions : class
    {
        services.AddScoped(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
            return sp.GetRequiredKeyedService<TService>(keySelector(options));
        });
    }

    private static IServiceCollection AddNexovaOptions(this IServiceCollection services)
    {
        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName);
        services.AddOptions<FileSystemStorageOptions>()
            .BindConfiguration(FileSystemStorageOptions.SectionName);
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName);

        services.AddSingleton<IValidateOptions<StorageOptions>, StorageOptionsValidator>();
        services.AddSingleton<IValidateOptions<FileSystemStorageOptions>, FileSystemStorageOptionsValidator>();
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        return services;
    }
}