using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.Models;

public class Room
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public BingoType Type { get; set; }
    public int MaxPlayers { get; set; } = 50;
    public bool IsPrivate { get; set; }
    public string? InviteCode { get; set; }
    [Required]
    public Guid HostId { get; set; }
    public User? Host { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<BingoCard> Cards { get; set; } = new List<BingoCard>();
    public ICollection<GameSession> Sessions { get; set; } = new List<GameSession>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}