using Microsoft.AspNetCore.Mvc;
using BingoGameApi.DTOs;
using BingoGameApi.Services;

namespace BingoGameApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var userDto = await _authService.RegisterAsync(registerDto);
            return CreatedAtAction(nameof(Register), new { id = userDto.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var tokenDto = await _authService.LoginAsync(loginDto);
            return Ok(tokenDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("guest")]
    public async Task<ActionResult<string>> Guest()
    {
        var token = await _authService.GenerateGuestTokenAsync();
        return Ok(token);
    }
}