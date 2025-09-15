using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using System.Linq;
using BingoGameApi.Models;

namespace BingoGameApi.Models;

public class BingoDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<BingoCard> BingoCards { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    public BingoDbContext(DbContextOptions<BingoDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.InviteCode).HasMaxLength(20);
            entity.Property(e => e.MaxPlayers).HasDefaultValue(50);
            entity.HasIndex(e => e.InviteCode).IsUnique();
            entity.HasOne(r => r.Host)
                  .WithMany()
                  .HasForeignKey(r => r.HostId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(r => r.Cards)
                  .WithOne(c => c.Room)
                  .HasForeignKey(c => c.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(r => r.Sessions)
                  .WithOne(s => s.Room)
                  .HasForeignKey(s => s.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(r => r.Messages)
                  .WithOne(m => m.Room)
                  .HasForeignKey(m => m.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // BingoCard
        modelBuilder.Entity<BingoCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PlayerId);
            entity.Property(e => e.AssignedCount).HasDefaultValue(1);
            entity.HasOne(c => c.Player)
                  .WithMany()
                  .HasForeignKey(c => c.PlayerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // JSON for Numbers
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            entity.Property(e => e.Numbers)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, jsonOptions),
                      v => JsonSerializer.Deserialize<List<int>>(v, jsonOptions)!,
                      new ValueComparer<List<int>>(
                          (c1, c2) => c1.SequenceEqual(c2),
                          c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c.ToList()))
                  .HasColumnType("nvarchar(max)");

            // JSON for Marks
            var jsonOptionsMarks = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            entity.Property(e => e.Marks)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, jsonOptionsMarks),
                      v => JsonSerializer.Deserialize<Dictionary<string, bool>>(v, jsonOptionsMarks)!,
                      new ValueComparer<Dictionary<string, bool>>(
                          (c1, c2) => c1.OrderBy(kv => kv.Key).SequenceEqual(c2.OrderBy(kv => kv.Key)),
                          c => c.Aggregate(0, (a, kv) => HashCode.Combine(a, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
                          c => c.ToDictionary(kv => kv.Key, kv => kv.Value)))
                  .HasColumnType("nvarchar(max)");
        });

        // GameSession
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.WinnerId);
            entity.HasOne(s => s.Winner)
                  .WithMany()
                  .HasForeignKey(s => s.WinnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // JSON for DrawnBalls
            var jsonOptionsDrawn = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            entity.Property(e => e.DrawnBalls)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, jsonOptionsDrawn),
                      v => JsonSerializer.Deserialize<List<int>>(v, jsonOptionsDrawn)!,
                      new ValueComparer<List<int>>(
                          (c1, c2) => c1.SequenceEqual(c2),
                          c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c.ToList()))
                  .HasColumnType("nvarchar(max)");
        });

        // ChatMessage
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
            entity.Property(e => e.SenderId).IsRequired();
            entity.HasOne(m => m.Sender)
                  .WithMany()
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}