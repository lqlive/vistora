using Apache.DataFusion;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class MySqlConnector : IConnector
{
    public DataSourceType Type => DataSourceType.MySql;

    public Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.MySqlOptions(dataSource.Configuration, tableName);

        context.RegisterMySql(tableName, options);
        return Task.CompletedTask;
    }
}