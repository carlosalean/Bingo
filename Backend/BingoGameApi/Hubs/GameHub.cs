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

[Authorize]
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
        var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            var roomIdStr = Context.GetHttpContext().Request.Query["roomId"].ToString();
            if (Guid.TryParse(roomIdStr, out var roomId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");
            }
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
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            var userId = Guid.Parse(userIdClaim.Value);
            var name = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";
    
            // Validate token if guest - stub for now (assume guest token provides temp userId via claims)
            // if (!string.IsNullOrEmpty(userToken))
            // {
            //     // Validate guest token, set temp userId if needed
            //     // e.g., using IAuthService.ValidateGuestToken(userToken)
            // }
    
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");
            await Clients.Group($"room-{roomId}").SendAsync("PlayerJoined", new { UserId = userId, Name = name });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoom: {ex.Message}");
            throw;
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
    
            var userId = Guid.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var name = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Guest";
    
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
    
            var senderId = Guid.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
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

    public async Task GetDrawnBalls(string roomIdStr)
    {
        try
        {
            if (!Guid.TryParse(roomIdStr, out var roomId))
            {
                throw new ArgumentException("Invalid roomId");
            }
    
            var drawnBalls = await _gameService.GetDrawnBallsAsync(roomId);
            await Clients.Caller.SendAsync("DrawnBalls", new { List = drawnBalls });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDrawnBalls: {ex.Message}");
            throw;
        }
    }
}