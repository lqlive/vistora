namespace Nexova.Charts.Models;

public sealed record ChartResponse(
    Guid Id,
    string Name,
    string VizType,
    string Dataset,
    string? Description,
    string? Configuration,
    bool Favorite,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
