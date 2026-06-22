using Apache.DataFusion;
using Apache.DataFusion.TableProviders.ClickHouse;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class ClickHouseConnector : IConnector
{
    public DataSourceType Type => DataSourceType.ClickHouse;

    public Task RegisterAsync(SessionContext context, string sourceName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.ClickHouseOptions(dataSource.Configuration, sourceName);

        context.RegisterClickHouse(options);
        return Task.CompletedTask;
    }
}
