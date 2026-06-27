using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ChatSystem.Models;
public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    public int RoomId { get; set; }
    [ForeignKey("RoomId")]
    public ChatRoom Room { get; set; } = null!;

    public int SenderId { get; set; }
    [ForeignKey("SenderId")]
    public User Sender { get; set; } = null!;

    public string MessageText { get; set; } = null!;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    // Advanced Chat States
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }

    // Global Soft Delete (Hidden from everyone)
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}