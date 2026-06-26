namespace Nexova.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public ICollection<QueryDocument> QueryDocuments { get; set; } = [];
    public ICollection<WorkspaceConnection> WorkspaceConnections { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}