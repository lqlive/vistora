namespace Nexova.Core.Entities;

public class UserDataSource
{
    public Guid UserId { get; set; }
    public Guid DataSourceId { get; set; }
    public User User { get; set; } = null!;
    public DataSource DataSource { get; set; } = null!;
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}