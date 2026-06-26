using Microsoft.EntityFrameworkCore;
using Nexova.Core.Entities;
using Nexova.Core.Stores;

namespace Nexova.Database.PostgreSql;

public class PostgreSqlDashboardStore(PostgreSqlContext context) : IDashboardStore
{
    public async Task<bool> CreateAsync(Dashboard dashboard, CancellationToken cancellationToken)
    {
        if (await context.Dashboards.AnyAsync(value => value.Name == dashboard.Name, cancellationToken))
        {
            return false;
        }

        dashboard.CreatedAt = DateTimeOffset.UtcNow;
        dashboard.UpdatedAt = dashboard.CreatedAt;

        context.Dashboards.Add(dashboard);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<Dashboard?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Dashboards.FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Dashboard>> ListAsync(CancellationToken cancellationToken)
    {
        return await context.Dashboards
            .OrderBy(value => value.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken)
    {
        if (!await context.Dashboards.AnyAsync(value => value.Id == dashboard.Id, cancellationToken))
        {
            return false;
        }

        if (await context.Dashboards.AnyAsync(
            value => value.Id != dashboard.Id && value.Name == dashboard.Name,
            cancellationToken))
        {
            return false;
        }

        dashboard.UpdatedAt = DateTimeOffset.UtcNow;
        context.Dashboards.Update(dashboard);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var dashboard = await context.Dashboards.FindAsync([id], cancellationToken);
        if (dashboard is null)
        {
            return false;
        }

        context.Dashboards.Remove(dashboard);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
