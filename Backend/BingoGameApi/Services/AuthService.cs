using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BingoGameApi.DTOs;
using BingoGameApi.Models;
using BingoGameApi.Services;

namespace BingoGameApi.Services;

public class AuthService : IAuthService
{
    private readonly BingoDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(BingoDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }
    
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }
    
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = UserRole.Player
            };
    
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
    
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RegisterAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<TokenDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            User? user = null;
            if (loginDto.UsernameOrEmail.Contains("@"))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.UsernameOrEmail);
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.UsernameOrEmail);
            }
    
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
    
            var token = GenerateJwtToken(user.Id, user.Role.ToString());
    
            return new TokenDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoginAsync: {ex.Message}");
            throw;
        }
    }

    public Task<string> GenerateGuestTokenAsync()
    {
        var guestId = Guid.Empty;
        var guestRole = "Guest";
        return Task.FromResult(GenerateJwtToken(guestId, guestRole, TimeSpan.FromMinutes(5)));
    }

    private string GenerateJwtToken(Guid userId, string role, TimeSpan? expiry = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}