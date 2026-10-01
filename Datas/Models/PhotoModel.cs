using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSystem.Models;
public class PhotoModel
{
    [Key]
    public int PhotoId {get; set;}
    public string PhotoKey {get; set;} = null!;
    public string FileName {get; set;} = null!;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? UpdatedAt {get; set;} = null; 
}