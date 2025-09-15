using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.Models;

public class GameSession
{
    public Guid Id { get; set; }
    [Required]
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public List<int> DrawnBalls { get; set; } = new List<int>();
    public GameStatus Status { get; set; }
    public Guid? WinnerId { get; set; }
    public User? Winner { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}