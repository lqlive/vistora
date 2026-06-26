namespace Nexova.Charts.Models;

public sealed record ChartRequest(
    string Name,
    string VizType,
    string Dataset,
    string? Description,
    string? Configuration,
    bool Favorite);
