using BingoGameApi.DTOs;
using BingoGameApi.Models;
using BingoGameApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace BingoGameApi.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly BingoDbContext _context;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<BingoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BingoDbContext(options);
        _mockConfig = new Mock<IConfiguration>();
        SetupConfiguration();
        _authService = new AuthService(_context, _mockConfig.Object);
    }

    private void SetupConfiguration()
    {
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns("test-secret-key-with-minimum-length-of-32-chars");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("test-audience");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_ValidDto_ShouldCreateUserWithHashedPassword()
    {
        // Arrange
        var dto = new RegisterDto { Username = "testuser", Email = "test@example.com", Password = "TestPass123" };

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.NotNull(user);
        Assert.NotEqual("TestPass123", user.PasswordHash); // Hashed
        Assert.True(BCrypt.Net.BCrypt.Verify("TestPass123", user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_ExistingUsername_ShouldThrowException()
    {
        // Arrange
        var existingUser = new User { Id = Guid.NewGuid(), Username = "existing", Email = "existing@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"), Role = UserRole.Player };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto { Username = "existing", Email = "new@example.com", Password = "NewPass123" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(dto));
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ShouldThrowException()
    {
        // Arrange
        var existingUser = new User { Id = Guid.NewGuid(), Username = "new", Email = "existing@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"), Role = UserRole.Player };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto { Username = "newuser", Email = "existing@example.com", Password = "NewPass123" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnTokenDto()
    {
        // Arrange
        var password = "TestPass123";
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), Role = UserRole.Player };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { UsernameOrEmail = "testuser", Password = password };

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.User.Username);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var dto = new LoginDto { UsernameOrEmail = "nonexistent", Password = "wrongpass" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ValidEmail_ShouldReturnTokenDto()
    {
        // Arrange
        var password = "TestPass123";
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), Role = UserRole.Player };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto { UsernameOrEmail = "test@example.com", Password = password };

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal("testuser", result.User.Username);
    }

    [Fact]
    public async Task GenerateGuestTokenAsync_ShouldReturnToken()
    {
        // Act
        var token = await _authService.GenerateGuestTokenAsync();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        // Basic validation: token should be a JWT string
        Assert.True(token.Contains("."));
    }
}