using FluentValidation;
using BingoGameApi.DTOs;
using BingoGameApi.Models;

namespace BingoGameApi.Validators;

public class RoomCreateDtoValidator : AbstractValidator<RoomCreateDto>
{
    public RoomCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.MaxPlayers)
            .InclusiveBetween(2, 20);

        // BingoType and IsPrivate have no specific validation
    }
}