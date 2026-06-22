using Apache.DataFusion;
using Nexova.Core.Entities;

namespace Nexova.Connectors.Abstractions;

public interface IConnector
{
    DataSourceType Type { get; }

    Task RegisterAsync(SessionContext context, string sourceName, DataSource dataSource,
        CancellationToken cancellationToken);
}
