namespace Nexova.Dashboards.Models;

public sealed record DashboardRequest(
    string Name,
    string? Status,
    string? Description,
    string? Configuration,
    bool Favorite);
