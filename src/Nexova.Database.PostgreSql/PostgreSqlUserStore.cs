using Nexova.Core.Entities;
using Nexova.Core.Stores;

namespace Nexova.Database.PostgreSql;

public class PostgreSqlUserStore : IUserStore
{
    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User> UpsertByEmailAsync(User user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
