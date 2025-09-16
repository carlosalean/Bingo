using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.Models;

public class Invitation
{
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public Guid RoomId { get; set; }
    
    [Required]
    public Guid InvitedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsUsed { get; set; } = false;
    
    public DateTime? UsedAt { get; set; }
    
    public string? GuestName { get; set; }
    
    // Navigation properties
    public Room Room { get; set; } = null!;
    public User InvitedBy { get; set; } = null!;
}