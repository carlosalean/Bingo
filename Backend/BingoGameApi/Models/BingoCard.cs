using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BingoGameApi.Models;

public class BingoCard
{
    public Guid Id { get; set; }
    [Required]
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public Guid? PlayerId { get; set; }
    public User? Player { get; set; }
    public BingoType Type { get; set; }
    public List<int> Numbers { get; set; } = new List<int>();
    public Dictionary<string, bool> Marks { get; set; } = new Dictionary<string, bool>();
    public int AssignedCount { get; set; } = 1;
}