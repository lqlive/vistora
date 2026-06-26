using System.Collections.Concurrent;
using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public class InMemoryDashboardStore : IDashboardStore
{
    private static readonly ConcurrentDictionary<Guid, Dashboard> dashboards = [];

    public Task<bool> CreateAsync(Dashboard dashboard, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (dashboards.Values.Any(value => value.Name == dashboard.Name))
        {
            return Task.FromResult(false);
        }

        dashboard.CreatedAt = DateTimeOffset.UtcNow;
        dashboard.UpdatedAt = dashboard.CreatedAt;
        return Task.FromResult(dashboards.TryAdd(dashboard.Id, dashboard));
    }

    public Task<Dashboard?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        dashboards.TryGetValue(id, out var dashboard);
        return Task.FromResult(dashboard);
    }

    public Task<IReadOnlyList<Dashboard>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Dashboard> items = dashboards.Values
            .OrderBy(value => value.Name)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<bool> UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!dashboards.ContainsKey(dashboard.Id))
        {
            return Task.FromResult(false);
        }

        if (dashboards.Values.Any(value => value.Id != dashboard.Id && value.Name == dashboard.Name))
        {
            return Task.FromResult(false);
        }

        dashboard.UpdatedAt = DateTimeOffset.UtcNow;
        dashboards[dashboard.Id] = dashboard;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(dashboards.TryRemove(id, out _));
    }
}
