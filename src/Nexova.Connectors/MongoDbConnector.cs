using Apache.DataFusion;
using Apache.DataFusion.TableProviders.MongoDB;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class MongoDbConnector : IConnector
{
    public DataSourceType Type => DataSourceType.MongoDb;

    public Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        var options = DataSourceConnectionStrings.MongoDbOptions(dataSource.Configuration, dataSource.Name);

        context.RegisterMongoDb(options);
        return Task.CompletedTask;
    }
}