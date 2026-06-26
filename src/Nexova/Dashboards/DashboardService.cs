using ErrorOr;
using Nexova.Core.Entities;
using Nexova.Core.Stores;
using Nexova.Dashboards.Errors;
using Nexova.Dashboards.Models;

namespace Nexova.Dashboards;

public sealed class DashboardService(IDashboardStore dashboardStore)
{
    public async Task<IReadOnlyList<DashboardResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var dashboards = await dashboardStore.ListAsync(cancellationToken);
        return dashboards.Select(dashboard => dashboard.ToResponse()).ToList();
    }

    public async Task<ErrorOr<DashboardResponse>> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await dashboardStore.GetAsync(id, cancellationToken);
        if (dashboard is null)
        {
            return DashboardErrors.NotFound;
        }

        return dashboard.ToResponse();
    }

    public async Task<ErrorOr<DashboardResponse>> CreateAsync(
        DashboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Status = NormalizeStatus(request.Status),
            Description = Normalize(request.Description),
            Configuration = Normalize(request.Configuration),
            Favorite = request.Favorite
        };

        var created = await dashboardStore.CreateAsync(dashboard, cancellationToken);
        if (!created)
        {
            return DashboardErrors.NameAlreadyExists;
        }

        return dashboard.ToResponse();
    }

    public async Task<ErrorOr<DashboardResponse>> UpdateAsync(
        Guid id,
        DashboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await dashboardStore.GetAsync(id, cancellationToken);
        if (dashboard is null)
        {
            return DashboardErrors.NotFound;
        }

        dashboard.Name = request.Name.Trim();
        dashboard.Status = NormalizeStatus(request.Status);
        dashboard.Description = Normalize(request.Description);
        dashboard.Configuration = Normalize(request.Configuration);
        dashboard.Favorite = request.Favorite;

        var updated = await dashboardStore.UpdateAsync(dashboard, cancellationToken);
        if (!updated)
        {
            return DashboardErrors.NameAlreadyExists;
        }

        return dashboard.ToResponse();
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await dashboardStore.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return DashboardErrors.NotFound;
        }

        return Result.Deleted;
    }

    private static string NormalizeStatus(string? status) =>
        string.Equals(status?.Trim(), "published", StringComparison.OrdinalIgnoreCase)
            ? "published"
            : "draft";

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
