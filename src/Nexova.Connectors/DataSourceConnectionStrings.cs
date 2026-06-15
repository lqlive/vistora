using Apache.DataFusion;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

internal static class DataSourceConnectionStrings
{
    public static PostgresTableOptions PostgreSqlOptions(DataSourceConfiguration configuration, string tableName) =>
        new(PostgreSql(configuration), tableName)
        {
            SchemaName = Optional(configuration.Schema)
        };

    public static MySqlTableOptions MySqlOptions(DataSourceConfiguration configuration, string tableName) =>
        new(MySql(configuration), tableName)
        {
            SchemaName = Optional(configuration.Schema)
        };

    public static ClickHouseTableOptions ClickHouseOptions(DataSourceConfiguration configuration, string tableName) =>
        new(ClickHouseUrl(configuration), tableName)
        {
            Database = Optional(configuration.Database),
            User = Optional(configuration.Username),
            Password = configuration.Password
        };

    public static string PostgreSql(DataSourceConfiguration configuration) =>
        ExistingOrBuildDatabaseUri(configuration, "postgresql", 5432);

    public static string MySql(DataSourceConfiguration configuration) =>
        ExistingOrBuildDatabaseUri(configuration, "mysql", 3306);

    public static string ClickHouseUrl(DataSourceConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString.Trim();
        }

        var host = Required(configuration.Host, nameof(configuration.Host));
        var port = configuration.Port ?? 8123;
        var scheme = configuration.Options?.GetValueOrDefault("scheme") ?? "http";

        return $"{scheme}://{host}:{port}";
    }

    private static string ExistingOrBuildDatabaseUri(
        DataSourceConfiguration configuration,
        string scheme,
        int defaultPort)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString.Trim();
        }

        var host = Required(configuration.Host, nameof(configuration.Host));
        var database = Required(configuration.Database, nameof(configuration.Database));
        var port = configuration.Port ?? defaultPort;
        var credentials = BuildCredentials(configuration);

        return $"{scheme}://{credentials}{host}:{port}/{Uri.EscapeDataString(database)}";
    }

    private static string BuildCredentials(DataSourceConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.Username))
        {
            return string.Empty;
        }

        var username = Uri.EscapeDataString(configuration.Username.Trim());
        if (string.IsNullOrEmpty(configuration.Password))
        {
            return $"{username}@";
        }

        return $"{username}:{Uri.EscapeDataString(configuration.Password)}@";
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
