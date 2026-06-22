using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Nexova.Users.Models;

namespace Nexova.Users.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddGitHubAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(GitHubAuthOptions.SectionName);
        services.Configure<GitHubAuthOptions>(section);
        var options = section.Get<GitHubAuthOptions>() ?? new GitHubAuthOptions();

        services.AddAuthentication(authentication =>
            {
                authentication.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(cookie =>
            {
                cookie.Cookie.Name = "nexova.auth";
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.ExpireTimeSpan = TimeSpan.FromDays(7);
                cookie.SlidingExpiration = true;
                cookie.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                cookie.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddGitHub(github =>
            {
                github.ClientId = options.ClientId ?? string.Empty;
                github.ClientSecret = options.ClientSecret ?? string.Empty;
                github.CallbackPath = options.CallbackPath;

                github.Scope.Add("read:user");
                github.Scope.Add("user:email");
                github.SaveTokens = true;

                github.Events.OnCreatingTicket = OnCreatingTicketAsync;
            });

        services.AddAuthorization();
        return services;
    }

    private static async Task OnCreatingTicketAsync(OAuthCreatingTicketContext context)
    {
        var profile = new GitHubProfile(
            ReadString(context.User, "login") ?? string.Empty,
            ReadString(context.User, "name"),
            context.Identity?.FindFirst(ClaimTypes.Email)?.Value ?? ReadString(context.User, "email"),
            ReadString(context.User, "avatar_url"));

        var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
        var user = await userService.UpsertFromGitHubAsync(profile, context.HttpContext.RequestAborted);

        context.Identity?.AddClaim(new Claim(AuthenticationConstants.UserIdClaimType, user.Id.ToString()));
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
