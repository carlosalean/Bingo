using BingoGameApi.Models;

namespace BingoGameApi.DTOs;

public class BingoCardDto
{
    public Guid Id { get; set; }
    public List<int> Numbers { get; set; } = new List<int>();
    public Dictionary<string, bool> Marks { get; set; } = new Dictionary<string, bool>();
    public BingoType Type { get; set; }
    public Guid? PlayerId { get; set; }
    public string? PlayerName { get; set; }
}