using Nexova.Core.Entities;

namespace Nexova.Dashboards.Models;

public static class DashboardMappingExtensions
{
    public static DashboardResponse ToResponse(this Dashboard dashboard) =>
        new(
            dashboard.Id,
            dashboard.Name,
            dashboard.Status,
            dashboard.Description,
            dashboard.Configuration,
            dashboard.Favorite,
            dashboard.CreatedAt,
            dashboard.UpdatedAt);
}
