using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public interface IChartStore
{
    Task<bool> CreateAsync(Chart chart, CancellationToken cancellationToken);
    Task<Chart?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Chart>> ListAsync(CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Chart chart, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
