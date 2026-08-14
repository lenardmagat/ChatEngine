using System.ComponentModel.DataAnnotations;
namespace ChatSystem.Models;
public enum Roles
{
    Admin,
    User
}
public class User
{
    [Key]
    public int UserId {get; set;}
    
    public required string Username {get; set;}
    public required string HashedPassword {get; set;}
    public required Roles Role {get; set;}
    public required bool Status {get; set;}
}