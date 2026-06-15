using Apache.DataFusion;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class MongoDbConnector : IConnector
{
    public DataSourceType Type => DataSourceType.MongoDb;

    public Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = new MongoDbTableOptions(
            dataSource.Configuration.ConnectionString!,
            tableName);

        context.RegisterMongoDb(tableName, options);
        return Task.CompletedTask;
    }
}