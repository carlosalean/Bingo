using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.DTOs;

public class JoinRoomDto
{
    [Required]
    public string InviteCode { get; set; } = string.Empty;

    public int NumCards { get; set; } = 1;
}