using Microsoft.EntityFrameworkCore;
using ChatSystem.Models;
namespace ChatSystem.DataBase;
public class DbManager : DbContext
{
    public DbSet<User> Users {get; set;} = null!;
    public DbSet<ChatMessage> Messages {get; set;} = null!;
    public DbSet<ChatRoom> Chatrooms {get; set;} = null!;
    public DbSet<RoomParticipant> participants {get; set;} = null!;
    public  DbManager(DbContextOptions<DbManager> options) : base(options){}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}