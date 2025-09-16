using BingoGameApi.DTOs;
using BingoGameApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BingoGameApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpPost("create")]
    [Authorize(Roles = "Player,Host")]
    public async Task<ActionResult<InvitationDto>> CreateInvitation([FromBody] CreateInvitationDto createInvitationDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user token");

            var invitation = await _invitationService.CreateInvitationAsync(createInvitationDto, userId);
            return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id }, invitation);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvitationDto>> GetInvitation(Guid id)
    {
        try
        {
            var invitation = await _invitationService.GetInvitationByIdAsync(id);
            return Ok(invitation);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("room/{roomId}")]
    [Authorize(Roles = "Player,Host")]
    public async Task<ActionResult<List<InvitationDto>>> GetRoomInvitations(Guid roomId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user token");

            var invitations = await _invitationService.GetRoomInvitationsAsync(roomId, userId);
            return Ok(invitations);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("accept")]
    public async Task<ActionResult<TokenDto>> AcceptInvitation([FromBody] AcceptInvitationDto acceptInvitationDto)
    {
        try
        {
            var token = await _invitationService.AcceptInvitationAsync(acceptInvitationDto);
            return Ok(token);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Player,Host")]
    public async Task<ActionResult> DeleteInvitation(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user token");

            var deleted = await _invitationService.DeleteInvitationAsync(id, userId);
            if (!deleted)
                return NotFound("Invitation not found");

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}/valid")]
    public async Task<ActionResult<bool>> IsInvitationValid(Guid id)
    {
        var isValid = await _invitationService.IsInvitationValidAsync(id);
        return Ok(isValid);
    }
}