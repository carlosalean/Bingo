using BingoGameApi.DTOs;
using BingoGameApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BingoGameApi.Services;

public interface IGameService
{
    Task<List<BingoCardDto>> GenerateUniqueCardsAsync(Guid roomId, int numCards, Guid? playerId, BingoType type);
    Task<Guid> StartGameAsync(Guid roomId);
    Task<int> DrawBallAsync(Guid roomId);
    Task<List<int>> GetDrawnBallsAsync(Guid roomId);
    bool IsHost(Guid userId, Guid roomId);
    Task<BingoGameApi.DTOs.WinDto?> UpdateMarkAsync(Guid roomId, Guid cardId, string position, bool marked);

    Task<BingoGameApi.DTOs.WinDto?> CheckWinAsync(Guid cardId, List<int> drawnBalls, bool useClientMarks = false);

    Task<BingoGameApi.DTOs.WinDto?> CheckWinsAfterDrawAsync(Guid roomId);

    Task PauseGameAsync(Guid roomId);

    Task EndGameAsync(Guid roomId);
}