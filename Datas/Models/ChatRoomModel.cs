using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ChatSystem.Models;
public class ChatRoom
{
    [Key]
    public int Id { get; set; }
    
    public string? Name { get; set; } // Null if it's a private 1-on-1 DM channel
    public bool IsGroupChat { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Soft Delete Indicator
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<RoomParticipant> Participants { get; set; } = new List<RoomParticipant>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}