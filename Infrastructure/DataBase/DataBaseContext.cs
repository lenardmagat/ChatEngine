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
    public DbSet<SaleOfferEvent> SaleOfferEvents {get; set;} = null!;
    public DbSet<Product> Products {get; set;} = null!;
    public  DbManager(DbContextOptions<DbManager> options) : base(options){}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SaleOfferEvent>()
            .HasIndex(e => new { e.SaleOfferId, e.Version });

        modelBuilder.Entity<SaleOfferEvent>()
            .HasOne(e => e.SaleOffer)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.SaleOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SaleOfferEvent>()
            .HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}