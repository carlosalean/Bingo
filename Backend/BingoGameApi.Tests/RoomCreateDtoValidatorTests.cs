using FluentValidation.TestHelper;
using BingoGameApi.DTOs;
using BingoGameApi.Models;
using BingoGameApi.Validators;
using Xunit;

namespace BingoGameApi.Tests;

public class RoomCreateDtoValidatorTests
{
    private readonly RoomCreateDtoValidator _validator;

    public RoomCreateDtoValidatorTests()
    {
        _validator = new RoomCreateDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new RoomCreateDto { Name = string.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var model = new RoomCreateDto { Name = new string('a', 101) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var model = new RoomCreateDto { Name = "Valid Room Name" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(1)] // Below minimum
    [InlineData(21)] // Above maximum
    public void Should_Have_Error_When_MaxPlayers_Is_Outside_Valid_Range(int maxPlayers)
    {
        var model = new RoomCreateDto { MaxPlayers = maxPlayers };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.MaxPlayers);
    }

    [Theory]
    [InlineData(2)] // Minimum valid
    [InlineData(10)] // Middle range
    [InlineData(20)] // Maximum valid
    public void Should_Not_Have_Error_When_MaxPlayers_Is_Within_Valid_Range(int maxPlayers)
    {
        var model = new RoomCreateDto { MaxPlayers = maxPlayers };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.MaxPlayers);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Properties_Are_Valid()
    {
        var model = new RoomCreateDto
        {
            Name = "Test Room",
            BingoType = BingoType.SeventyFive,
            MaxPlayers = 10,
            IsPrivate = false
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}