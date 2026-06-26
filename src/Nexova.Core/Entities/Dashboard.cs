namespace Nexova.Core.Entities;

public class Dashboard
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string Status { get; set; } = "draft";
    public string? Description { get; set; }
    public string? Configuration { get; set; }
    public bool Favorite { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
