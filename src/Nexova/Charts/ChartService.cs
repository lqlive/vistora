using ErrorOr;
using Nexova.Charts.Errors;
using Nexova.Charts.Models;
using Nexova.Core.Entities;
using Nexova.Core.Stores;

namespace Nexova.Charts;

public sealed class ChartService(IChartStore chartStore)
{
    public async Task<IReadOnlyList<ChartResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var charts = await chartStore.ListAsync(cancellationToken);
        return charts.Select(chart => chart.ToResponse()).ToList();
    }

    public async Task<ErrorOr<ChartResponse>> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var chart = await chartStore.GetAsync(id, cancellationToken);
        if (chart is null)
        {
            return ChartErrors.NotFound;
        }

        return chart.ToResponse();
    }

    public async Task<ErrorOr<ChartResponse>> CreateAsync(
        ChartRequest request,
        CancellationToken cancellationToken = default)
    {
        var chart = new Chart
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            VizType = request.VizType.Trim(),
            Dataset = request.Dataset.Trim(),
            Description = Normalize(request.Description),
            Configuration = Normalize(request.Configuration),
            Favorite = request.Favorite
        };

        var created = await chartStore.CreateAsync(chart, cancellationToken);
        if (!created)
        {
            return ChartErrors.NameAlreadyExists;
        }

        return chart.ToResponse();
    }

    public async Task<ErrorOr<ChartResponse>> UpdateAsync(
        Guid id,
        ChartRequest request,
        CancellationToken cancellationToken = default)
    {
        var chart = await chartStore.GetAsync(id, cancellationToken);
        if (chart is null)
        {
            return ChartErrors.NotFound;
        }

        chart.Name = request.Name.Trim();
        chart.VizType = request.VizType.Trim();
        chart.Dataset = request.Dataset.Trim();
        chart.Description = Normalize(request.Description);
        chart.Configuration = Normalize(request.Configuration);
        chart.Favorite = request.Favorite;

        var updated = await chartStore.UpdateAsync(chart, cancellationToken);
        if (!updated)
        {
            return ChartErrors.NameAlreadyExists;
        }

        return chart.ToResponse();
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await chartStore.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return ChartErrors.NotFound;
        }

        return Result.Deleted;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
