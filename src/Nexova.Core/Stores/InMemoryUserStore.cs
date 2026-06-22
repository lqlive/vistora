using System.Collections.Concurrent;
using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public class InMemoryUserStore : IUserStore
{
    private static readonly ConcurrentDictionary<Guid, User> users = [];

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = users.Values.FirstOrDefault(candidate => MatchesEmail(candidate, email));
        return Task.FromResult(user);
    }

    public Task<User> UpsertByEmailAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(user);

        var existing = string.IsNullOrWhiteSpace(user.Email)
            ? null
            : users.Values.FirstOrDefault(candidate => MatchesEmail(candidate, user.Email!));

        if (existing is null)
        {
            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            user.CreatedAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = user.CreatedAt;
            users[user.Id] = user;
            return Task.FromResult(user);
        }

        existing.Name = user.Name;
        existing.Email = user.Email;
        existing.AvatarUrl = user.AvatarUrl;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        users[existing.Id] = existing;
        return Task.FromResult(existing);
    }

    private static bool MatchesEmail(User user, string email) =>
        !string.IsNullOrWhiteSpace(user.Email)
        && string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
}
