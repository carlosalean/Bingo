using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BingoGameApi.Services;
using BingoGameApi.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BingoGameApi.Hubs;

[AllowAnonymous]
public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly IRoomService _roomService;

    public GameHub(IGameService gameService, IRoomService roomService)
    {
        _gameService = gameService;
        _roomService = roomService;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"SignalR connection established: {Context.ConnectionId}");
        Console.WriteLine($"User authenticated: {Context.User?.Identity?.IsAuthenticated}");
        Console.WriteLine($"User name: {Context.User?.Identity?.Name}");
        
        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier);
        Console.WriteLine($"User ID claim: {userIdClaim?.Value}");
        
        if (userIdClaim != null)
        {
            var roomIdStr = Context.GetHttpContext().Request.Query["roomId"].ToString();
            Console.WriteLine($"Room ID from query: {roomIdStr}");
            
            if (Guid.TryParse(roomIdStr, out var roomId))
            {
                Console.WriteLine($"Adding connection {Context.ConnectionId} to room-{roomId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");
            }
            else
            {
                Console.WriteLine($"Invalid room ID format in query: {roomIdStr}");
            }
        }
        else
        {
            Console.WriteLine("No user ID claim found in connection");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var roomIdStr = Context.GetHttpContext().Request.Query["roomId"].ToString();
        if (Guid.TryParse(roomIdStr, out var roomId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room-{roomId}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomIdStr, string? userToken = null)
    {
        try
        {
            Console.WriteLine($"JoinRoom called with roomIdStr: {roomIdStr}");
            
            if (string.IsNullOrEmpty(roomIdStr))
            {
                Console.WriteLine("RoomId is null or empty");
                await Clients.Caller.SendAsync("Error", "Room ID is required");
                return;
            }
            
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                Console.WriteLine($"Invalid roomId format: {roomIdStr}");
                await Clients.Caller.SendAsync("Error", "Invalid room ID format");
                return;
            }
    
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            Guid userId;
            string name;
            
            Console.WriteLine($"User claims: {Context.User?.Identity?.Name}, IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");
            
            // Handle guest users and parsing errors
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value) || !Guid.TryParse(userIdClaim.Value, out userId) || userId == Guid.Empty)
            {
                userId = Guid.NewGuid(); // Generate temporary ID for guest
                name = "Guest";
                Console.WriteLine($"Using guest user with ID: {userId}");
            }
            else
            {
                name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";
                Console.WriteLine($"Using authenticated user: {name} with ID: {userId}");
            }
    
            // Validate token if guest - stub for now (assume guest token provides temp userId via claims)
            // if (!string.IsNullOrEmpty(userToken))
            // {
            //     // Validate guest token, set temp userId if needed
            //     // e.g., using IAuthService.ValidateGuestToken(userToken)
            // }
    
            Console.WriteLine($"Adding user {userId} to group room-{roomId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");
            await Clients.Group($"room-{roomId}").SendAsync("PlayerJoined", new { UserId = userId, Name = name });
            
            Console.WriteLine($"Successfully joined room {roomId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoom: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            await Clients.Caller.SendAsync("Error", $"Failed to join room: {ex.Message}");
        }
    }

    public async Task LeaveRoom(string roomIdStr)
    {
        try
        {
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            Guid userId;
            string name;
            
            // Handle guest users and parsing errors
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value) || !Guid.TryParse(userIdClaim.Value, out userId) || userId == Guid.Empty)
            {
                userId = Guid.NewGuid(); // Generate temporary ID for guest
                name = "Guest";
            }
            else
            {
                name = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";
            }
    
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room-{roomId}");
            await Clients.Group($"room-{roomId}").SendAsync("PlayerLeft", new { UserId = userId, Name = name });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LeaveRoom: {ex.Message}");
            throw;
        }
    }

    public async Task SendChat(string roomIdStr, string message)
    {
        try
        {
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            Guid senderId;
            
            // Handle guest users and parsing errors
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value) || !Guid.TryParse(userIdClaim.Value, out senderId) || senderId == Guid.Empty)
            {
                senderId = Guid.NewGuid(); // Generate temporary ID for guest
            }
    
            await _roomService.AddChatMessageAsync(roomId, senderId, message);
    
            var timestamp = DateTime.UtcNow;
            await Clients.Group($"room-{roomId}").SendAsync("NewMessage", new { SenderId = senderId, Message = message, Timestamp = timestamp });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendChat: {ex.Message}");
            throw;
        }
    }

    public async Task MarkNumber(string roomIdStr, string cardIdStr, string position, bool marked)
    {
        try
        {
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            if (!Guid.TryParse(cardIdStr, out var cardId))
            {
                throw new ArgumentException("Invalid cardId");
            }
    
            // Optional: validate ownership
            var win = await _gameService.UpdateMarkAsync(roomId, cardId, position, marked);
    
            await Clients.Group($"room-{roomId}").SendAsync("CardMarked", new { CardId = cardId, Position = position, Marked = marked });
    
            if (win != null)
            {
                await Clients.Group($"room-{roomId}").SendAsync("NotifyWin", win);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MarkNumber: {ex.Message}");
            throw;
        }
    }

    public async Task NotifyWin(string roomIdStr, string winnerIdStr)
    {
        try
        {
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            if (!Guid.TryParse(winnerIdStr, out var winnerId))
            {
                throw new ArgumentException("Invalid winnerId");
            }
    
            // Stub for win detection logic
            await Clients.Group($"room-{roomId}").SendAsync("WinDetected", new { WinnerId = winnerId, Pattern = "line" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in NotifyWin: {ex.Message}");
            throw;
        }
    }

    public async Task<object> GetDrawnBalls(string roomIdStr)
    {
        try
        {
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            var drawnBalls = await _gameService.GetDrawnBallsAsync(roomId);
            return new { balls = drawnBalls };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDrawnBalls: {ex.Message}");
            throw;
        }
    }
}