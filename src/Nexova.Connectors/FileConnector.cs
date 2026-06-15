using Apache.DataFusion;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class FileConnector : IConnector
{
    public DataSourceType Type => DataSourceType.Files;

    public Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        foreach (var asset in dataSource.FileAssets)
        {
            
        }
        return Task.CompletedTask;
    }
}