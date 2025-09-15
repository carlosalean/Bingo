using BingoGameApi.Models;

namespace BingoGameApi.DTOs;

public class RoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BingoType Type { get; set; }
    public int MaxPlayers { get; set; }
    public bool IsPrivate { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public Guid HostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PlayerCount { get; set; }
}