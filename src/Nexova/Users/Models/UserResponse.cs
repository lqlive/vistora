namespace Nexova.Users.Models;

public sealed record UserResponse(
    Guid Id,
    string Name,
    string? Email,
    string? AvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
