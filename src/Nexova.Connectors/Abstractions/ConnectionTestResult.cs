namespace Nexova.Connectors.Abstractions;

public sealed record ConnectionTestResult(
    bool Success,
    string? Message,
    long ElapsedMilliseconds);
