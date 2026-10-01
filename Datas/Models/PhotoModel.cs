using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSystem.Models;
public class PhotoModel
{
    [Key]
    public int PhotoId {get; set;}
    public string PhotoKey {get; set;} = null!;
    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime? UpdatedAt = null;
    public int? UserId;
    [ForeignKey("UserId")]
    public User? UserProfilePhoto = null;
    public int? ProductId;
    [ForeignKey("ProductId")]
    public Product? ProductPhoto = null; 
}