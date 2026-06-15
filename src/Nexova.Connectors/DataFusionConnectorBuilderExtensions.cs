using Microsoft.Extensions.DependencyInjection;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;
using Nexova.Core.Management;

namespace Nexova.Connectors;

public static class DataFusionConnectorBuilderExtensions
{
    public static INexovaBuilder AddDataFusionConnectors(this INexovaBuilder builder)
    {
        var services = builder.Services;

        services.AddKeyedScoped<IConnector, FileConnector>(DataSourceType.Files);
        services.AddKeyedScoped<IConnector, MySqlConnector>(DataSourceType.MySql);
        services.AddKeyedScoped<IConnector, PostgreSqlConnector>(DataSourceType.PostgreSql);
        services.AddKeyedScoped<IConnector, ClickHouseConnector>(DataSourceType.ClickHouse);
        services.AddKeyedScoped<IConnector, MongoDbConnector>(DataSourceType.MongoDb);

        services.AddScoped<IQueryExecutor, DataFusionQueryExecutor>();
        return builder;
    }
}