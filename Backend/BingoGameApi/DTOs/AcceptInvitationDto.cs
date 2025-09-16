using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.DTOs;

public class AcceptInvitationDto
{
    [Required]
    public Guid InvitationId { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string GuestName { get; set; } = string.Empty;
}