using FluentValidation;
using BingoGameApi.DTOs;

namespace BingoGameApi.Validators;

public class JoinRoomDtoValidator : AbstractValidator<JoinRoomDto>
{
    public JoinRoomDtoValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty();

        RuleFor(x => x.NumCards)
            .InclusiveBetween(1, 3);
    }
}