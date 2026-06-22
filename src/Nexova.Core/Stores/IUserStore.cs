using Nexova.Core.Entities;

namespace Nexova.Core.Stores;

public interface IUserStore
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User> UpsertByEmailAsync(User user, CancellationToken cancellationToken);
}
