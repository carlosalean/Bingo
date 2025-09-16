using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.DTOs;

public class CreateInvitationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public Guid RoomId { get; set; }
}