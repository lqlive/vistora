using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public interface IDashboardStore
{
    Task<bool> CreateAsync(Dashboard dashboard, CancellationToken cancellationToken);
    Task<Dashboard?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Dashboard>> ListAsync(CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
