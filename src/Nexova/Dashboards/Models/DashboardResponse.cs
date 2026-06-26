namespace Nexova.Dashboards.Models;

public sealed record DashboardResponse(
    Guid Id,
    string Name,
    string Status,
    string? Description,
    string? Configuration,
    bool Favorite,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
