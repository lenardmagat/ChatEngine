using System.ComponentModel.DataAnnotations;

namespace ChatSystem.Models;
public enum NotificationType
{
    Message,
    User,
    System
}
public class Notification
{
    [Key]
    public long Id { get; set;}
    public int UserId { get; set;}
    public NotificationType Type { get; set;}
    public string Message { get; set; } = null!;
    public string? TargetId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}