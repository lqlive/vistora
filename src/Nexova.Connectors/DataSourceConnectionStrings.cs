using Apache.DataFusion;
using Apache.DataFusion.TableProviders.ClickHouse;
using Apache.DataFusion.TableProviders.MongoDB;
using Apache.DataFusion.TableProviders.MySql;
using Apache.DataFusion.TableProviders.PostgreSql;
using Apache.DataFusion.TableProviders.Sqlite;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

internal static class DataSourceConnectionStrings
{
    public static PostgreSqlDatabaseOptions PostgreSqlOptions(DataSourceConfiguration configuration, string sourceName) =>
        new()
        {
            ConnectionString = PostgreSql(configuration),
            SourceName = sourceName,
            Schemas = Optional(configuration.Schema) is { } schema ? [schema] : ["public"]
        };

    public static MySqlDatabaseOptions MySqlOptions(DataSourceConfiguration configuration, string sourceName) =>
        new()
        {
            ConnectionString = MySql(configuration),
            SourceName = sourceName,
            DatabaseName = Optional(configuration.Database)
        };

    public static ClickHouseDatabaseOptions ClickHouseOptions(DataSourceConfiguration configuration, string sourceName) =>
        new()
        {
            ConnectionString = ClickHouse(configuration),
            SourceName = sourceName,
            DatabaseName = Optional(configuration.Database)
        };

    public static MongoDbDatabaseOptions MongoDbOptions(DataSourceConfiguration configuration, string sourceName) =>
        new()
        {
            ConnectionString = MongoDb(configuration),
            SourceName = sourceName,
            DatabaseName = Optional(configuration.Database)
        };

    public static SqliteDatabaseOptions SqliteOptions(DataSourceConfiguration configuration, string sourceName) =>
        new()
        {
            ConnectionString = Sqlite(configuration),
            SourceName = sourceName
        };

    public static string PostgreSql(DataSourceConfiguration configuration) =>
        ExistingOrBuildDatabaseConnectionString(configuration, "Host", 5432);

    public static string MySql(DataSourceConfiguration configuration) =>
        ExistingOrBuildDatabaseConnectionString(configuration, "Server", 3306);

    public static string MongoDb(DataSourceConfiguration configuration) =>
        Required(configuration.ConnectionString, nameof(configuration.ConnectionString));

    public static string Sqlite(DataSourceConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString.Trim();
        }

        var path = Optional(configuration.Path) ?? Required(configuration.StoragePath, nameof(configuration.StoragePath));
        return BuildConnectionString(("Data Source", path));
    }

    public static string ClickHouse(DataSourceConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString.Trim();
        }

        var host = Required(configuration.Host, nameof(configuration.Host));
        var port = configuration.Port ?? 8123;
        var database = Optional(configuration.Database);
        var protocol = Optional(configuration.Options?.GetValueOrDefault("protocol"))
            ?? Optional(configuration.Options?.GetValueOrDefault("scheme"))
            ?? "https";

        return BuildConnectionString(
            ("Host", host),
            ("Port", port),
            ("Protocol", protocol),
            ("Username", Optional(configuration.Username) ?? "default"),
            ("Password", configuration.Password),
            ("Database", database));
    }

    private static string ExistingOrBuildDatabaseConnectionString(
        DataSourceConfiguration configuration,
        string hostKey,
        int defaultPort)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString.Trim();
        }

        var host = Required(configuration.Host, nameof(configuration.Host));
        var database = Required(configuration.Database, nameof(configuration.Database));
        var port = configuration.Port ?? defaultPort;

        return BuildConnectionString(
            (hostKey, host),
            ("Port", port),
            ("Database", database),
            ("User ID", Optional(configuration.Username)),
            ("Password", configuration.Password));
    }

    private static string BuildConnectionString(params (string Key, object? Value)[] values)
    {
        var builder = new System.Data.Common.DbConnectionStringBuilder();
        foreach (var (key, value) in values)
        {
            if (value is string { Length: 0 } or null)
            {
                continue;
            }

            builder[key] = value;
        }

        return builder.ConnectionString;
    }

    private static string Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} is required.", name);
        }

        return value.Trim();
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
