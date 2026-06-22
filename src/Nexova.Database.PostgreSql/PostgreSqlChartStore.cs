using Microsoft.EntityFrameworkCore;
using Nexova.Core.Entities;
using Nexova.Core.Stores;

namespace Nexova.Database.PostgreSql;

public class PostgreSqlChartStore(PostgreSqlContext context) : IChartStore
{
    public async Task<bool> CreateAsync(Chart chart, CancellationToken cancellationToken)
    {
        if (await context.Charts.AnyAsync(value => value.Name == chart.Name, cancellationToken))
        {
            return false;
        }

        chart.CreatedAt = DateTimeOffset.UtcNow;
        chart.UpdatedAt = chart.CreatedAt;

        context.Charts.Add(chart);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<Chart?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Charts.FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Chart>> ListAsync(CancellationToken cancellationToken)
    {
        return await context.Charts
            .OrderBy(value => value.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Chart chart, CancellationToken cancellationToken)
    {
        if (!await context.Charts.AnyAsync(value => value.Id == chart.Id, cancellationToken))
        {
            return false;
        }

        if (await context.Charts.AnyAsync(
            value => value.Id != chart.Id && value.Name == chart.Name,
            cancellationToken))
        {
            return false;
        }

        chart.UpdatedAt = DateTimeOffset.UtcNow;
        context.Charts.Update(chart);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var chart = await context.Charts.FindAsync([id], cancellationToken);
        if (chart is null)
        {
            return false;
        }

        context.Charts.Remove(chart);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
