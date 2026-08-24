using Microsoft.EntityFrameworkCore;
using ChatSystem.Models;
namespace ChatSystem.DataBase;
public class DbManager : DbContext
{
    public DbSet<User> Users {get; set;} = null!;
    public DbSet<ChatMessage> Messages {get; set;} = null!;
    public DbSet<ChatRoom> Chatrooms {get; set;} = null!;
    public DbSet<RoomParticipant> Participants {get; set;} = null!;
    public DbSet<TradeOffer> TradeOffers {get; set;} = null!;
    public DbSet<OutboxEntry> OutboxEntries {get; set;} = null!;
    public DbSet<RefreshToken> RefreshTokens {get; set;} = null!;
    public DbSet<SaleOffer> SaleOffers {get; set;} = null!;
    public DbSet<Product> Products {get; set;} = null!;
    public  DbManager(DbContextOptions<DbManager> options) : base(options){}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}