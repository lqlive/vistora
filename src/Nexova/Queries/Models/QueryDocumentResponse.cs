namespace Nexova.Queries.Models;

public sealed record QueryDocumentResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Sql,
    bool IsShared,
    bool IsOwner,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
