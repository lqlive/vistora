using Nexova.Connectors.Abstractions;

namespace Nexova.Queries.Models;

public sealed record ExplainResponse(
    string? LogicalPlan,
    string? PhysicalPlan,
    IReadOnlyList<ExplainPlanInfo> Plans,
    long DurationMs);
