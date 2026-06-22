using Apache.DataFusion;
using Apache.DataFusion.TableProviders.PostgreSql;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class PostgreSqlConnector : IConnector
{
    public DataSourceType Type => DataSourceType.PostgreSql;

    public Task RegisterAsync(SessionContext context, string sourceName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.PostgreSqlOptions(dataSource.Configuration, sourceName);

        context.RegisterPostgreSql(options);
        return Task.CompletedTask;
    }
}