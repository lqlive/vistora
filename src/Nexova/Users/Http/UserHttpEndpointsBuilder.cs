using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Nexova.Users.Authentication;
using Nexova.Users.Models;

namespace Nexova.Users.Http;

public static class UserHttpEndpointsBuilder
{
    public static RouteGroupBuilder MapAuthApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/user");
        api.MapGet("/github/login", LoginWithGitHub);
        api.MapGet("/me", Me);
        api.MapPost("/logout", Logout);

        return api;
    }

    private static IResult LoginWithGitHub([FromQuery] string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
        };

        return Results.Challenge(properties, [AuthenticationConstants.GitHubScheme]);
    }

    private static async Task<IResult> Me(
        ClaimsPrincipal principal,
        UserService userService,
        CancellationToken cancellationToken)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var userId = principal.FindFirstValue(AuthenticationConstants.UserIdClaimType);
        if (!Guid.TryParse(userId, out var id))
        {
            return Results.Unauthorized();
        }

        var user = await userService.GetAsync(id, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(user.ToResponse());
    }

    private static IResult Logout()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Results.SignOut(properties, [CookieAuthenticationDefaults.AuthenticationScheme]);
    }
}
