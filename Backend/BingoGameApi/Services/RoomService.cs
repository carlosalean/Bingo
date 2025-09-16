using BingoGameApi.DTOs;
using BingoGameApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BingoGameApi.Services;

public class RoomService : IRoomService
{
    private readonly BingoDbContext _context;
    private readonly Random _random = new Random();
    private readonly IGameService _gameService;

    public RoomService(BingoDbContext context, IGameService gameService)
    {
        _context = context;
        _gameService = gameService;
    }

    public async Task<RoomDto?> CreateRoomAsync(Guid userId, RoomCreateDto dto)
    {
        try
        {
            var inviteCode = GenerateUniqueInviteCode();
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Type = dto.BingoType,
                MaxPlayers = dto.MaxPlayers,
                IsPrivate = dto.IsPrivate,
                InviteCode = inviteCode,
                HostId = userId,
                CreatedAt = DateTime.UtcNow
            };
    
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
    
            await _gameService.GenerateUniqueCardsAsync(room.Id, 1, userId, room.Type);
    
            return MapToDto(room, 1); // Host as first player
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateRoomAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<List<RoomDto>> GetUserRoomsAsync(Guid userId)
    {
        try
        {
            var rooms = await _context.Rooms
                .Where(r => r.HostId == userId)
                .Include(r => r.Cards)
                .ToListAsync();
    
            return rooms.Select(r => MapToDto(r, GetPlayerCount(r.Cards))).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserRoomsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<RoomDto?> JoinRoomAsync(JoinRoomDto dto, Guid? userId)
    {
        try
        {
            if (dto.NumCards < 1 || dto.NumCards > 3)
            {
                throw new ArgumentException("NumCards must be between 1 and 3", nameof(dto.NumCards));
            }
    
            var room = await _context.Rooms
                .Include(r => r.Cards)
                .FirstOrDefaultAsync(r => r.InviteCode == dto.InviteCode);
    
            if (room == null)
            {
                throw new KeyNotFoundException("Room not found");
            }
    
            var currentPlayerCount = GetPlayerCount(room.Cards);
            if (currentPlayerCount >= room.MaxPlayers)
            {
                throw new InvalidOperationException("Room is full");
            }
    
            await _gameService.GenerateUniqueCardsAsync(room.Id, dto.NumCards, userId, room.Type);
    
            await _context.SaveChangesAsync();
    
            return MapToDto(room, currentPlayerCount + (userId.HasValue ? 1 : 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoomAsync: {ex.Message}");
            throw;
        }
    }

    private string GenerateUniqueInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        while (true)
        {
            var code = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
            if (!_context.Rooms.Any(r => r.InviteCode == code))
            {
                return code;
            }
        }
    }



    private static int GetPlayerCount(ICollection<BingoCard> cards)
    {
        return cards
            .Where(c => c.PlayerId.HasValue)
            .Select(c => c.PlayerId.Value)
            .Distinct()
            .Count();
    }

    private static RoomDto MapToDto(Room room, int playerCount)
    {
        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Type = room.Type,
            MaxPlayers = room.MaxPlayers,
            IsPrivate = room.IsPrivate,
            InviteCode = room.InviteCode ?? string.Empty,
            HostId = room.HostId,
            CreatedAt = room.CreatedAt,
            PlayerCount = playerCount
        };
    }

    public async Task AddChatMessageAsync(Guid roomId, Guid senderId, string message)
    {
        try
        {
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                SenderId = senderId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
    
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddChatMessageAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteRoomAsync(string roomId, Guid userId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return false;
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomGuid && r.HostId == userId);

            if (room == null)
            {
                return false;
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteRoomAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<DrawnBallsDto?> GetDrawnBallsAsync(string roomId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return null;
            }

            var drawnBalls = await _gameService.GetDrawnBallsAsync(roomGuid);
            return new DrawnBallsDto { DrawnBalls = drawnBalls };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDrawnBallsAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> StartGameAsync(string roomId, Guid userId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return false;
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomGuid && r.HostId == userId);

            if (room == null)
            {
                return false;
            }

            await _gameService.StartGameAsync(roomGuid);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartGameAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DrawBallAsync(string roomId, Guid userId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return false;
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomGuid && r.HostId == userId);

            if (room == null)
            {
                return false;
            }

            await _gameService.DrawBallAsync(roomGuid);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DrawBallAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PauseGameAsync(string roomId, Guid userId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return false;
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomGuid && r.HostId == userId);

            if (room == null)
            {
                return false;
            }

            await _gameService.PauseGameAsync(roomGuid);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PauseGameAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EndGameAsync(string roomId, Guid userId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return false;
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomGuid && r.HostId == userId);

            if (room == null)
            {
                return false;
            }

            await _gameService.EndGameAsync(roomGuid);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in EndGameAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<RoomDto?> JoinRoomByCodeAsync(string roomCode, Guid userId)
    {
        try
        {
            var room = await _context.Rooms
                .Include(r => r.Cards)
                .FirstOrDefaultAsync(r => r.InviteCode == roomCode);

            if (room == null)
            {
                return null;
            }

            var currentPlayerCount = GetPlayerCount(room.Cards);
            if (currentPlayerCount >= room.MaxPlayers)
            {
                throw new InvalidOperationException("Room is full");
            }

            await _gameService.GenerateUniqueCardsAsync(room.Id, 1, userId, room.Type);
            await _context.SaveChangesAsync();

            return MapToDto(room, currentPlayerCount + 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoomByCodeAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<RoomDto?> JoinRoomByIdAsync(string roomId, Guid userId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return null;
            }

            var room = await _context.Rooms
                .Include(r => r.Cards)
                .FirstOrDefaultAsync(r => r.Id == roomGuid);

            if (room == null)
            {
                return null;
            }

            var currentPlayerCount = GetPlayerCount(room.Cards);
            if (currentPlayerCount >= room.MaxPlayers)
            {
                throw new InvalidOperationException("Room is full");
            }

            await _gameService.GenerateUniqueCardsAsync(room.Id, 1, userId, room.Type);
            await _context.SaveChangesAsync();

            return MapToDto(room, currentPlayerCount + 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoomByIdAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<RoomDto?> GetRoomByCodeAsync(string roomCode)
    {
        try
        {
            var room = await _context.Rooms
                .Include(r => r.Cards)
                .FirstOrDefaultAsync(r => r.InviteCode == roomCode);

            if (room == null)
            {
                return null;
            }

            var currentPlayerCount = GetPlayerCount(room.Cards);
            return MapToDto(room, currentPlayerCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRoomByCodeAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<RoomDto?> GetRoomByIdAsync(string roomId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return null;
            }

            var room = await _context.Rooms
                .Include(r => r.Cards)
                .FirstOrDefaultAsync(r => r.Id == roomGuid);

            if (room == null)
            {
                return null;
            }

            var currentPlayerCount = GetPlayerCount(room.Cards);
            return MapToDto(room, currentPlayerCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRoomByIdAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<RoomDto>> GetPublicRoomsAsync()
    {
        try
        {
            var rooms = await _context.Rooms
                .Include(r => r.Cards)
                .Where(r => !r.IsPrivate)
                .ToListAsync();

            return rooms.Select(room => MapToDto(room, GetPlayerCount(room.Cards))).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPublicRoomsAsync: {ex.Message}");
            return new List<RoomDto>();
        }
    }

    public async Task<List<BingoCardDto>> GetRoomPlayersAsync(string roomId)
    {
        try
        {
            if (!Guid.TryParse(roomId, out var roomGuid))
            {
                return new List<BingoCardDto>();
            }

            var cards = await _context.BingoCards
                .Include(c => c.Player)
                .Where(c => c.RoomId == roomGuid)
                .ToListAsync();

            return cards.Select(card => new BingoCardDto
            {
                Id = card.Id,
                Numbers = card.Numbers,
                Marks = card.Marks,
                Type = card.Type,
                PlayerId = card.PlayerId,
                PlayerName = card.Player?.Username ?? "Guest"
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRoomPlayersAsync: {ex.Message}");
            return new List<BingoCardDto>();
        }
    }
}