using System.ComponentModel.DataAnnotations;
using BingoGameApi.Models;

namespace BingoGameApi.DTOs;

public class RoomCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public BingoType BingoType { get; set; }

    public int MaxPlayers { get; set; } = 50;

    public bool IsPrivate { get; set; } = false;
}