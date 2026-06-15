using Nexova.Connectors.Abstractions;

namespace Nexova.Query.Models;

public sealed record ExplainResponse(
    string? LogicalPlan,
    string? PhysicalPlan,
    IReadOnlyList<ExplainPlanInfo> Plans,
    long DurationMs);
