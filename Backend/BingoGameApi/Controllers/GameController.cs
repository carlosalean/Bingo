using BingoGameApi.DTOs;
using BingoGameApi.Hubs;
using BingoGameApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BingoGameApi.Controllers;

[ApiController]
[Route("api/game")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IHubContext<GameHub> _hubContext;

    public GameController(IGameService gameService, IHubContext<GameHub> hubContext)
    {
        _gameService = gameService;
        _hubContext = hubContext;
    }

    [HttpPost("start/{roomId}")]
    public async Task<IActionResult> StartGame(Guid roomId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (!_gameService.IsHost(userId, roomId))
            {
                return Forbid("Only the host can start the game");
            }
    
            var sessionId = await _gameService.StartGameAsync(roomId);
            return Ok(new { sessionId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartGame: {ex.Message}");
            return BadRequest("Failed to start game");
        }
    }

    [HttpPost("pause/{roomId}")]
    public async Task<IActionResult> PauseGame(Guid roomId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (!_gameService.IsHost(userId, roomId))
            {
                return Forbid("Only the host can pause the game");
            }
    
            await _gameService.PauseGameAsync(roomId);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PauseGame: {ex.Message}");
            return BadRequest("Failed to pause game");
        }
    }

    [HttpPost("end/{roomId}")]
    public async Task<IActionResult> EndGame(Guid roomId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (!_gameService.IsHost(userId, roomId))
            {
                return Forbid("Only the host can end the game");
            }
    
            await _gameService.EndGameAsync(roomId);
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in EndGame: {ex.Message}");
            return BadRequest("Failed to end game");
        }
    }

    [HttpPost("draw/{roomId}")]
    public async Task<IActionResult> DrawBall(Guid roomId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (!_gameService.IsHost(userId, roomId))
            {
                return Forbid("Only the host can draw balls");
            }
    
            var ball = await _gameService.DrawBallAsync(roomId);
            await _hubContext.Clients.Group($"room-{roomId}").SendAsync("BallDrawn", ball);
    
            var win = await _gameService.CheckWinsAfterDrawAsync(roomId);
            if (win != null)
            {
                await _hubContext.Clients.Group($"room-{roomId}").SendAsync("NotifyWin", win);
            }
    
            return Ok(new { ball });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DrawBall: {ex.Message}");
            if (ex.Message.Contains("No more balls"))
            {
                return Conflict(ex.Message);
            }
            return BadRequest("Failed to draw ball");
        }
    }

    [HttpGet("drawn/{roomId}")]
    public async Task<IActionResult> GetDrawnBalls(Guid roomId)
    {
        try
        {
            var drawnBalls = await _gameService.GetDrawnBallsAsync(roomId);
            return Ok(new DrawnBallsDto { DrawnBalls = drawnBalls });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDrawnBalls: {ex.Message}");
            return BadRequest("Failed to get drawn balls");
        }
    }
}