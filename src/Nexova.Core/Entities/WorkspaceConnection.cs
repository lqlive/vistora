namespace Nexova.Core.Entities;

public class WorkspaceConnection
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DataSourceId { get; set; }
    public User User { get; set; } = null!;
    public DataSource DataSource { get; set; } = null!;
    public string? Alias { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}