using Apache.DataFusion;
using Apache.DataFusion.TableProviders.MySql;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class MySqlConnector : IConnector
{
    public DataSourceType Type => DataSourceType.MySql;

    public Task RegisterAsync(SessionContext context, string sourceName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.MySqlOptions(dataSource.Configuration, sourceName);

        context.RegisterMySql(options);
        return Task.CompletedTask;
    }
}