using Nexova.Core.Entities;

namespace Nexova.DataSources.Models;

public sealed record DataSourceResponse(
    Guid Id,
    string Name,
    string Type,
    DataSourceConfiguration Configuration,
    IReadOnlyList<DataSourceFileResponse> Files,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
