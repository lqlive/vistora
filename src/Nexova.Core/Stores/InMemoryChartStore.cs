using System.Collections.Concurrent;
using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public class InMemoryChartStore : IChartStore
{
    private static readonly ConcurrentDictionary<Guid, Chart> charts = [];

    public Task<bool> CreateAsync(Chart chart, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (charts.Values.Any(value => value.Name == chart.Name))
        {
            return Task.FromResult(false);
        }

        chart.CreatedAt = DateTimeOffset.UtcNow;
        chart.UpdatedAt = chart.CreatedAt;
        return Task.FromResult(charts.TryAdd(chart.Id, chart));
    }

    public Task<Chart?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        charts.TryGetValue(id, out var chart);
        return Task.FromResult(chart);
    }

    public Task<IReadOnlyList<Chart>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Chart> items = charts.Values
            .OrderBy(value => value.Name)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<bool> UpdateAsync(Chart chart, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!charts.ContainsKey(chart.Id))
        {
            return Task.FromResult(false);
        }

        if (charts.Values.Any(value => value.Id != chart.Id && value.Name == chart.Name))
        {
            return Task.FromResult(false);
        }

        chart.UpdatedAt = DateTimeOffset.UtcNow;
        charts[chart.Id] = chart;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(charts.TryRemove(id, out _));
    }
}
