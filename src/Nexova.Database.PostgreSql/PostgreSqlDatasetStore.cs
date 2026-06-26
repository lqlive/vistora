using Nexova.Core.Entities;
using Nexova.Core.Stores;
using Microsoft.EntityFrameworkCore;

namespace Nexova.Database.PostgreSql;

public class PostgreSqlDatasetStore(PostgreSqlContext context) : IDatasetStore
{
    public async Task<bool> CreateAsync(Dataset dataset, CancellationToken cancellationToken)
    {
        if (await context.Datasets.AnyAsync(value => value.Name == dataset.Name, cancellationToken))
        {
            return false;
        }

        dataset.CreatedAt = DateTimeOffset.UtcNow;
        dataset.UpdatedAt = dataset.CreatedAt;

        context.Datasets.Add(dataset);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var dataset = await context.Datasets.FindAsync([id], cancellationToken);
        if (dataset is null)
        {
            return false;
        }

        context.Datasets.Remove(dataset);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<Dataset?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Datasets.FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Dataset>> ListAsync(CancellationToken cancellationToken)
    {
        return await context.Datasets
            .OrderBy(value => value.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Dataset dataset, CancellationToken cancellationToken)
    {
        if (!await context.Datasets.AnyAsync(value => value.Id == dataset.Id, cancellationToken))
        {
            return false;
        }

        if (await context.Datasets.AnyAsync(
            value => value.Id != dataset.Id && value.Name == dataset.Name,
            cancellationToken))
        {
            return false;
        }

        dataset.UpdatedAt = DateTimeOffset.UtcNow;
        context.Datasets.Update(dataset);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}