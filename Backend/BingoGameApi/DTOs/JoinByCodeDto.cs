using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.DTOs;

public class JoinByCodeDto
{
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código de sala debe tener exactamente 6 caracteres")]
    [RegularExpression(@"^[A-Z]{3}[0-9]{3}$", ErrorMessage = "El código debe tener el formato ABC123 (3 letras seguidas de 3 números)")]
    public string RoomCode { get; set; } = string.Empty;
}