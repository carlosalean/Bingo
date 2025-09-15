using System;

namespace BingoGameApi.DTOs;

public class WinDto
{
    public Guid CardId { get; set; }
    public Guid PlayerId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public bool IsFull { get; set; }
}