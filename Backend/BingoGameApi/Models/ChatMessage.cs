using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.Models;

public class ChatMessage
{
    public Guid Id { get; set; }
    [Required]
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    [Required]
    public Guid SenderId { get; set; }
    public User? Sender { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}