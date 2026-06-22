using Apache.DataFusion;
using Apache.DataFusion.TableProviders.Sqlite;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class SqliteConnector : IConnector
{
    public DataSourceType Type => DataSourceType.Sqlite;

    public Task RegisterAsync(SessionContext context, string sourceName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.SqliteOptions(dataSource.Configuration, sourceName);

        context.RegisterSqlite(options);
        return Task.CompletedTask;
    }
}
