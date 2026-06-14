using Nexova.Core.Configuration;
using Nexova.Core.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Nexova.Core.Management;

public static partial class NexovaServiceCollectionExtensions
{
    public static INexovaBuilder AddFileStorage(this INexovaBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;
        services.AddKeyedTransient<IStorageService, FileStorageService>(FileSystemStorageOptions.Name);

        return builder;
    }
}
