using ChatSystem.DTOs.Documentation;

public class OutboxEntry
{
    public int Id { get; set; }
    public required DocumentTarget EntityType { get; set; }   // "User" for now
    public required int EntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }         // null = still pending
}