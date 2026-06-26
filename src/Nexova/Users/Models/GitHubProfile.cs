namespace Nexova.Users.Models;

public sealed record GitHubProfile(
    string Login,
    string? Name,
    string? Email,
    string? AvatarUrl);
