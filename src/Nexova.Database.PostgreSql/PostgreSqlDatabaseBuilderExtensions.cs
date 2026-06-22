using Nexova.Core.Configuration;
using Nexova.Core.Management;
using Nexova.Core.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Nexova.Database.PostgreSql;

public static class PostgreSqlDatabaseBuilderExtensions
{
    public const string Name = "PostgreSql";

    public static INexovaBuilder AddPostgreSqlDatabase(this INexovaBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddDbContextProvider<
            PostgreSqlContext,
            PostgreSqlDataSourceStore,
            PostgreSqlDatasetStore,
            PostgreSqlChartStore>(
            Name,
            (provider, options) =>
            {
                var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseNpgsql(databaseOptions.ConnectionString);
            });

        builder.Services.AddKeyedScoped<IUserStore, PostgreSqlUserStore>(Name);
        builder.Services.AddKeyedScoped<IQueryDocumentStore, PostgreSqlQueryDocumentStore>(Name);

        return builder;
    }
}
