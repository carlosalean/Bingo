using FluentValidation;
using BingoGameApi.DTOs;

namespace BingoGameApi.Validators;

public class JoinByCodeDtoValidator : AbstractValidator<JoinByCodeDto>
{
    public JoinByCodeDtoValidator()
    {
        RuleFor(x => x.RoomCode)
            .NotEmpty()
            .WithMessage("El código de sala es requerido");

        RuleFor(x => x.RoomCode)
            .Length(6)
            .WithMessage("El código de sala debe tener exactamente 6 caracteres");

        RuleFor(x => x.RoomCode)
            .Matches(@"^[A-Z]{3}[0-9]{3}$")
            .WithMessage("El código debe tener el formato ABC123 (3 letras seguidas de 3 números)");
    }
}