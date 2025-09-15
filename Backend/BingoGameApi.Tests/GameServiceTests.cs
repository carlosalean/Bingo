using BingoGameApi.DTOs;
using BingoGameApi.Models;
using BingoGameApi.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BingoGameApi.Tests;

public class GameServiceTests : IDisposable
{
    private readonly BingoDbContext _context;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        var options = new DbContextOptionsBuilder<BingoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BingoDbContext(options);
        _gameService = new GameService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GenerateUniqueCardsAsync_ShouldGenerateUniqueCardsFor75Balls()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Type = BingoType.SeventyFive };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        // Act
        var cards = await _gameService.GenerateUniqueCardsAsync(roomId, 2, null, BingoType.SeventyFive);

        // Assert
        Assert.Equal(2, cards.Count);
        foreach (var card in cards)
        {
            Assert.Equal(25, card.Numbers.Count);
            Assert.Contains(card.Numbers, n => n == 0); // Center free
            var nonZero = card.Numbers.Where(n => n > 0).ToList();
            Assert.All(nonZero, n => Assert.InRange(n, 1, 75));
            // Note: Exact column validation would require more complex checks
        }
        // Check uniqueness
        var hashes = cards.Select(c => string.Join(",", c.Numbers.Where(n => n > 0).OrderBy(n => n))).Distinct().ToList();
        Assert.Equal(2, hashes.Count);
    }

    [Fact]
    public async Task GenerateUniqueCardsAsync_ShouldGenerateUniqueCardsFor90Balls()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, Type = BingoType.Ninety };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        // Act
        var cards = await _gameService.GenerateUniqueCardsAsync(roomId, 2, null, BingoType.Ninety);

        // Assert
        Assert.Equal(2, cards.Count);
        foreach (var card in cards)
        {
            Assert.Equal(15, card.Numbers.Count);
            var nonZero = card.Numbers.Where(n => n > 0).ToList();
            Assert.All(nonZero, n => Assert.InRange(n, 1, 90));
        }
        // Check uniqueness
        var hashes = cards.Select(c => string.Join(",", c.Numbers.OrderBy(n => n))).Distinct().ToList();
        Assert.Equal(2, hashes.Count);
    }

    [Fact]
    public async Task CheckWinAsync_75Balls_RowWin_ShouldReturnWinDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new BingoCard
        {
            Id = cardId,
            Numbers = Enumerable.Range(1, 25).ToList(), // Dummy numbers
            Marks = Enumerable.Range(0, 25).ToDictionary(i => i.ToString(), i => false),
            Type = BingoType.SeventyFive
        };
        _context.BingoCards.Add(card);
        await _context.SaveChangesAsync();

        var drawnBalls = new List<int> { 1, 2, 3, 4, 5 }; // First row numbers assuming sequential

        // Act
        var win = await _gameService.CheckWinAsync(cardId, drawnBalls, useClientMarks: false);

        // Assert
        // Note: Since numbers are sequential, drawn 1-5 marks first row positions 0-4
        Assert.NotNull(win);
        Assert.Equal("row1", win.Pattern);
        Assert.False(win.IsFull);
    }

    [Fact]
    public async Task CheckWinAsync_75Balls_FullWin_ShouldReturnWinDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new BingoCard
        {
            Id = cardId,
            Numbers = Enumerable.Range(1, 25).ToList(),
            Marks = Enumerable.Range(0, 25).ToDictionary(i => i.ToString(), i => true), // All marked
            Type = BingoType.SeventyFive
        };
        _context.BingoCards.Add(card);
        await _context.SaveChangesAsync();

        var drawnBalls = Enumerable.Range(1, 75).ToList(); // All possible drawn

        // Act
        var win = await _gameService.CheckWinAsync(cardId, drawnBalls, useClientMarks: true);

        // Assert
        Assert.NotNull(win);
        Assert.Equal("full", win.Pattern);
        Assert.True(win.IsFull);
    }

    [Fact]
    public async Task CheckWinAsync_90Balls_LineWin_ShouldReturnWinDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new BingoCard
        {
            Id = cardId,
            Numbers = Enumerable.Range(1, 15).ToList(),
            Type = BingoType.Ninety
        };
        _context.BingoCards.Add(card);
        await _context.SaveChangesAsync();

        var drawnBalls = new List<int> { 1, 2, 3, 4, 5 }; // First line

        // Act
        var win = await _gameService.CheckWinAsync(cardId, drawnBalls, useClientMarks: false);

        // Assert
        Assert.NotNull(win);
        Assert.Equal("line1", win.Pattern);
        Assert.False(win.IsFull);
    }

    [Fact]
    public async Task CheckWinAsync_90Balls_TwoLinesWin_ShouldReturnWinDto()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new BingoCard
        {
            Id = cardId,
            Numbers = Enumerable.Range(1, 15).ToList(),
            Type = BingoType.Ninety
        };
        _context.BingoCards.Add(card);
        await _context.SaveChangesAsync();

        var drawnBalls = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; // First two lines

        // Act
        var win = await _gameService.CheckWinAsync(cardId, drawnBalls, useClientMarks: false);

        // Assert
        Assert.NotNull(win);
        Assert.Equal("two_lines", win.Pattern);
        Assert.False(win.IsFull);
    }

    [Fact]
    public async Task CheckWinAsync_NoWin_ShouldReturnNull()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new BingoCard
        {
            Id = cardId,
            Numbers = Enumerable.Range(1, 25).ToList(),
            Type = BingoType.SeventyFive
        };
        _context.BingoCards.Add(card);
        await _context.SaveChangesAsync();

        var drawnBalls = new List<int> { 1 }; // Only one drawn

        // Act
        var win = await _gameService.CheckWinAsync(cardId, drawnBalls, useClientMarks: false);

        // Assert
        Assert.Null(win);
    }
}