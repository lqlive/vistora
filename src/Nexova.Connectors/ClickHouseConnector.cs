using Apache.DataFusion;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class ClickHouseConnector : IConnector
{
    public DataSourceType Type => DataSourceType.ClickHouse;

    public Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.ClickHouseOptions(dataSource.Configuration, tableName);

        context.RegisterClickHouse(tableName, options);
        return Task.CompletedTask;
    }
}
