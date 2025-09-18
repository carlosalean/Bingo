using BingoGameApi.DTOs;
using BingoGameApi.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace BingoGameApi.Services;

public class InvitationService : IInvitationService
{
    private readonly BingoDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public InvitationService(BingoDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<InvitationDto> CreateInvitationAsync(CreateInvitationDto createInvitationDto, Guid invitedById)
    {
        // Verificar que la sala existe y el usuario es el host
        var room = await _context.Rooms
            .Include(r => r.Host)
            .FirstOrDefaultAsync(r => r.Id == createInvitationDto.RoomId);

        if (room == null)
            throw new ArgumentException("Room not found");

        if (room.HostId != invitedById)
            throw new UnauthorizedAccessException("Only room host can send invitations");

        // Verificar si ya existe una invitación para este email en esta sala
        var existingInvitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Email == createInvitationDto.Email && 
                                    i.RoomId == createInvitationDto.RoomId && 
                                    !i.IsUsed && 
                                    i.ExpiresAt > DateTime.UtcNow);

        if (existingInvitation != null)
            throw new InvalidOperationException("An active invitation already exists for this email in this room");

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            Email = createInvitationDto.Email,
            RoomId = createInvitationDto.RoomId,
            InvitedById = invitedById,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Expira en 7 días
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Generar enlace de invitación
        var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:4200";
        var invitationLink = $"{baseUrl}/join/{invitation.Id}";

        // Enviar email de invitación
        try
        {
            await _emailService.SendInvitationEmailAsync(
                invitation.Email,
                room.InviteCode, // Usar InviteCode en lugar del ID de invitación
                room.Name,
                room.Host.Username,
                invitationLink
            );
        }
        catch (Exception ex)
        {
            // Log el error pero no fallar la creación de la invitación
            Console.WriteLine($"Error sending invitation email: {ex.Message}");
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            RoomId = invitation.RoomId,
            RoomName = room.Name,
            InvitedByUsername = room.Host.Username,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed,
            UsedAt = invitation.UsedAt,
            GuestName = invitation.GuestName
        };
    }

    public async Task<InvitationDto> GetInvitationByIdAsync(Guid invitationId)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Room)
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
            throw new ArgumentException("Invitation not found");

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            RoomId = invitation.RoomId,
            RoomName = invitation.Room.Name,
            InvitedByUsername = invitation.InvitedBy.Username,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed,
            UsedAt = invitation.UsedAt,
            GuestName = invitation.GuestName
        };
    }

    public async Task<List<InvitationDto>> GetRoomInvitationsAsync(Guid roomId, Guid userId)
    {
        // Verificar que la sala existe y que el usuario es el host
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found");
        }

        if (room.HostId != userId)
        {
            throw new UnauthorizedAccessException("Only the room host can view invitations");
        }

        var invitations = await _context.Invitations
            .Include(i => i.Room)
            .Include(i => i.InvitedBy)
            .Where(i => i.RoomId == roomId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invitations.Select(invitation => new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            RoomId = invitation.RoomId,
            RoomName = invitation.Room.Name,
            InvitedByUsername = invitation.InvitedBy.Username,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed,
            UsedAt = invitation.UsedAt,
            GuestName = invitation.GuestName
        }).ToList();
    }

    public async Task<TokenDto> AcceptInvitationAsync(AcceptInvitationDto acceptInvitationDto)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Room)
            .FirstOrDefaultAsync(i => i.Id == acceptInvitationDto.InvitationId);

        if (invitation == null)
            throw new ArgumentException("Invitation not found");

        if (invitation.IsUsed)
            throw new InvalidOperationException("Invitation has already been used");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invitation has expired");

        // Crear usuario invitado temporal
        var guestUser = new User
        {
            Id = Guid.NewGuid(),
            Username = acceptInvitationDto.GuestName,
            Email = invitation.Email,
            PasswordHash = string.Empty, // Los invitados no tienen contraseña
            Role = UserRole.Guest
        };

        _context.Users.Add(guestUser);

        // Marcar invitación como usada
        invitation.IsUsed = true;
        invitation.UsedAt = DateTime.UtcNow;
        invitation.GuestName = acceptInvitationDto.GuestName;

        await _context.SaveChangesAsync();

        // Generar token JWT para el usuario invitado
        var token = GenerateJwtToken(guestUser);

        return new TokenDto
        {
            Token = token,
            ExpiresIn = DateTime.UtcNow.AddHours(1), // 1 hora
            User = new UserDto
            {
                Id = guestUser.Id,
                Username = guestUser.Username,
                Email = guestUser.Email,
                Role = guestUser.Role
            }
        };
    }

    public async Task<bool> DeleteInvitationAsync(Guid invitationId, Guid userId)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Room)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
            return false;

        // Solo el host de la sala puede eliminar invitaciones
        if (invitation.Room.HostId != userId)
            throw new UnauthorizedAccessException("Only room host can delete invitations");

        _context.Invitations.Remove(invitation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInvitationValidAsync(Guid invitationId)
    {
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        return invitation != null && 
               !invitation.IsUsed && 
               invitation.ExpiresAt > DateTime.UtcNow;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}