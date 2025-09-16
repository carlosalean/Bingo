using BingoGameApi.DTOs;
using BingoGameApi.Models;

namespace BingoGameApi.Services;
 
public interface IRoomService
{
    Task<RoomDto?> CreateRoomAsync(Guid userId, RoomCreateDto dto);
    Task<List<RoomDto>> GetUserRoomsAsync(Guid userId);
    Task<RoomDto?> JoinRoomAsync(JoinRoomDto dto, Guid? userId);
    Task AddChatMessageAsync(Guid roomId, Guid senderId, string message);
    Task<bool> DeleteRoomAsync(string roomId, Guid userId);
    Task<DrawnBallsDto?> GetDrawnBallsAsync(string roomId);
    Task<bool> StartGameAsync(string roomId, Guid userId);
    Task<bool> DrawBallAsync(string roomId, Guid userId);
    Task<bool> PauseGameAsync(string roomId, Guid userId);
    Task<bool> EndGameAsync(string roomId, Guid userId);
    Task<RoomDto?> JoinRoomByCodeAsync(string roomCode, Guid userId);
    Task<RoomDto?> JoinRoomByIdAsync(string roomId, Guid userId);
    Task<RoomDto?> GetRoomByCodeAsync(string roomCode);
    Task<RoomDto?> GetRoomByIdAsync(string roomId);
    Task<List<RoomDto>> GetPublicRoomsAsync();
    Task<List<BingoCardDto>> GetRoomPlayersAsync(string roomId);
}