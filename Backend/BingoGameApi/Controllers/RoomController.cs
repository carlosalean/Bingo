using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BingoGameApi.DTOs;
using BingoGameApi.Services;
using BingoGameApi.Models;

namespace BingoGameApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] RoomCreateDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
    
            var roomDto = await _roomService.CreateRoomAsync(userId.Value, dto);
            if (roomDto == null)
            {
                return NotFound("Room creation failed");
            }
    
            return CreatedAtAction(nameof(GetUserRooms), new { userId = userId.Value }, roomDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateRoom: {ex.Message}");
            return BadRequest("Failed to create room");
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<RoomDto>>> GetUserRooms()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
    
            var rooms = await _roomService.GetUserRoomsAsync(userId.Value);
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserRooms: {ex.Message}");
            return BadRequest("Failed to get rooms");
        }
    }

    [HttpPost("join")]
    [AllowAnonymous]
    public async Task<ActionResult<RoomDto>> JoinRoom([FromBody] JoinRoomDto dto)
    {
        try
        {
            var userId = GetUserId();
            var roomDto = await _roomService.JoinRoomAsync(dto, userId);
            if (roomDto == null)
            {
                return NotFound("Room not found");
            }
            return Ok(roomDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoom: {ex.Message}");
            return BadRequest("Failed to join room");
        }
    }

    [HttpDelete("{roomId}")]
    [Authorize]
    public async Task<ActionResult> DeleteRoom(string roomId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var success = await _roomService.DeleteRoomAsync(roomId, userId.Value);
            if (!success)
            {
                return NotFound("Room not found or unauthorized");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteRoom: {ex.Message}");
            return BadRequest("Failed to delete room");
        }
    }











    [HttpPost("join-by-code")]
    [Authorize]
    public async Task<ActionResult<RoomDto>> JoinRoomByCode([FromBody] JoinByCodeDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var roomDto = await _roomService.JoinRoomByCodeAsync(dto.RoomCode, userId.Value);
            if (roomDto == null)
            {
                return NotFound("Room not found");
            }

            return Ok(roomDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoomByCode: {ex.Message}");
            return BadRequest("Failed to join room");
        }
    }

    [HttpPost("{roomId}/join")]
    [Authorize]
    public async Task<ActionResult<RoomDto>> JoinRoomById(string roomId)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }

            var roomDto = await _roomService.JoinRoomByIdAsync(roomId, userId.Value);
            if (roomDto == null)
            {
                return NotFound("Room not found");
            }

            return Ok(roomDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoomById: {ex.Message}");
            return BadRequest("Failed to join room");
        }
    }

    [HttpGet("by-code/{roomCode}")]
    [Authorize]
    public async Task<ActionResult<RoomDto>> GetRoomByCode(string roomCode)
    {
        try
        {
            var roomDto = await _roomService.GetRoomByCodeAsync(roomCode);
            if (roomDto == null)
            {
                return NotFound("Room not found");
            }

            return Ok(roomDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRoomByCode: {ex.Message}");
            return BadRequest("Failed to get room");
        }
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RoomDto>>> GetPublicRooms()
    {
        try
        {
            var rooms = await _roomService.GetPublicRoomsAsync();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPublicRooms: {ex.Message}");
            return BadRequest("Failed to get public rooms");
        }
    }
}