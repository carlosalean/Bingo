using BingoGameApi.DTOs;

namespace BingoGameApi.Services;

public interface IInvitationService
{
    Task<InvitationDto> CreateInvitationAsync(CreateInvitationDto createInvitationDto, Guid invitedById);
    Task<InvitationDto> GetInvitationByIdAsync(Guid invitationId);
    Task<List<InvitationDto>> GetRoomInvitationsAsync(Guid roomId, Guid userId);
    Task<TokenDto> AcceptInvitationAsync(AcceptInvitationDto acceptInvitationDto);
    Task<bool> DeleteInvitationAsync(Guid invitationId, Guid userId);
    Task<bool> IsInvitationValidAsync(Guid invitationId);
}