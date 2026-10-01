using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ChatSystem.Models;
public enum Roles
{
    Admin,
    User,
    System
}
public class User
{
    [Key]
    public int UserId {get; set;}
    
    public required string Username {get; set;}
    public required string HashedPassword {get; set;}
    public required Roles Role {get; set;}
    public required bool Status {get; set;}
    public int? PhotoId {get; set;}= null;
    [ForeignKey("PhotoId")]
    public PhotoModel? UserProfilePicture {get; set;} 
    
}