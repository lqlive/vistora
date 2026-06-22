using Nexova.Core.Entities;

namespace Nexova.Charts.Models;

public static class ChartMappingExtensions
{
    public static ChartResponse ToResponse(this Chart chart) =>
        new(
            chart.Id,
            chart.Name,
            chart.VizType,
            chart.Dataset,
            chart.Description,
            chart.Configuration,
            chart.Favorite,
            chart.CreatedAt,
            chart.UpdatedAt);
}
