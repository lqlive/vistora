namespace Nexova.Users.Authentication;

public sealed class GitHubAuthOptions
{
    public const string SectionName = "Authentication:GitHub";

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string CallbackPath { get; set; } = "/api/auth/github/callback";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
