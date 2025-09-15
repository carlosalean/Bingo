using BingoGameApi.DTOs;
using BingoGameApi.Models;

namespace BingoGameApi.DTOs;

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}