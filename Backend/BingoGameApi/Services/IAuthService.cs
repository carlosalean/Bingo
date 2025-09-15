using BingoGameApi.DTOs;
using BingoGameApi.Models;

namespace BingoGameApi.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
    Task<TokenDto> LoginAsync(LoginDto loginDto);
    Task<string> GenerateGuestTokenAsync();
}