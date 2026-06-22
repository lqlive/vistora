using Nexova.Core.Entities;
using Nexova.Core.Stores;
using Nexova.Users.Models;

namespace Nexova.Users;

public sealed class UserService(IUserStore userStore)
{
    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        userStore.GetAsync(id, cancellationToken);

    public Task<User> UpsertFromGitHubAsync(
        GitHubProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var email = profile.Email?.Trim();
        var name = profile.Login;

        var user = new User
        {
            Name = name,
            Email = email,
            AvatarUrl = profile.AvatarUrl
        };

        return userStore.UpsertByEmailAsync(user, cancellationToken);
    }
}
