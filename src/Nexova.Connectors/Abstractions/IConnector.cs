using Apache.DataFusion;
using Nexova.Core.Entities;

namespace Nexova.Connectors.Abstractions;

public interface IConnector
{
    DataSourceType Type { get; }

    Task RegisterAsync(SessionContext context, string tableName, DataSource dataSource,
        CancellationToken cancellationToken);
}
