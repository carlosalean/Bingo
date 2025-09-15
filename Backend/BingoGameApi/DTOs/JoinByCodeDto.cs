using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.DTOs;

public class JoinByCodeDto
{
    [Required]
    [StringLength(10, MinimumLength = 4)]
    public string RoomCode { get; set; } = string.Empty;
}