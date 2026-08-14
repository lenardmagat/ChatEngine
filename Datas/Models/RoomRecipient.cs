using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ChatSystem.Models;
public class RoomParticipant
{
    [Key]
    public int Id { get; set; }

    public int RoomId { get; set; }
    [ForeignKey("RoomId")]
    public ChatRoom Room { get; set; } = null!;

    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsMuted { get; set; } = false;
    public DateTime? LastReadAt { get; set; }
}