using Apache.DataFusion;
using Apache.DataFusion.TableProviders.MySql;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class MySqlConnector : IConnector
{
    public DataSourceType Type => DataSourceType.MySql;

    public Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.MySqlOptions(dataSource.Configuration, dataSource.Name);

        context.RegisterMySql(options);
        return Task.CompletedTask;
    }
}