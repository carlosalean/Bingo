using BingoGameApi.DTOs;
using BingoGameApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoGameApi.Services;

public class GameService : IGameService
{
    private readonly BingoDbContext _context;
    private readonly Random _random;

    public GameService(BingoDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<List<BingoCardDto>> GenerateUniqueCardsAsync(Guid roomId, int numCards, Guid? playerId, BingoType type)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null) throw new ArgumentException("Room not found");

        var existingHashes = await _context.BingoCards
            .Where(c => c.RoomId == roomId)
            .Select(c => GetNumbersHash(c.Numbers))
            .ToListAsync();

        var generatedCards = new List<BingoCard>();
        for (int i = 0; i < numCards; i++)
        {
            BingoCard card;
            do
            {
                card = GenerateCard(type);
            } while (existingHashes.Contains(GetNumbersHash(card.Numbers)));

            card.Id = Guid.NewGuid();
            card.RoomId = roomId;
            card.PlayerId = playerId;
            card.Type = type;
            card.Marks = InitializeMarks(type);
            card.AssignedCount = 1;

            generatedCards.Add(card);
            existingHashes.Add(GetNumbersHash(card.Numbers));
        }

        _context.BingoCards.AddRange(generatedCards);
        await _context.SaveChangesAsync();

        return generatedCards.Select(c => new BingoCardDto
        {
            Id = c.Id,
            Numbers = c.Numbers,
            Marks = c.Marks,
            Type = c.Type
        }).ToList();
    }

    private string GetNumbersHash(List<int> numbers)
    {
        return string.Join(",", numbers.Where(n => n > 0).OrderBy(n => n));
    }

    private BingoCard GenerateCard(BingoType type)
    {
        var card = new BingoCard { Numbers = new List<int>() };
        if (type == BingoType.SeventyFive)
        {
            card.Numbers = Generate75Numbers();
        }
        else
        {
            card.Numbers = Generate90Numbers();
        }
        return card;
    }

    private List<int> Generate75Numbers()
    {
        var grid = new List<int>(new int[25]);
        var colPositions = new int[5][];
        colPositions[0] = new int[] {0,5,10,15,20};
        colPositions[1] = new int[] {1,6,11,16,21};
        colPositions[2] = new int[] {2,7,17,22}; // skip 12 for center free
        colPositions[3] = new int[] {3,8,13,18,23};
        colPositions[4] = new int[] {4,9,14,19,24};
        var ranges = new (int, int)[] { (1,15), (16,30), (31,45), (46,60), (61,75) };
        var counts = new int[] {5,5,4,5,5};

        for (int col = 0; col < 5; col++)
        {
            var nums = GenerateColumnNumbers(ranges[col].Item1, ranges[col].Item2, counts[col]);
            nums = nums.OrderBy(x => _random.Next()).ToList();
            for (int i = 0; i < nums.Count; i++)
            {
                int pos = colPositions[col][i];
                if (pos < 0 || pos >= grid.Count) throw new InvalidOperationException($"Invalid position {pos} for col {col}, i {i}");
                grid[pos] = nums[i];
            }
        }
        grid[12] = 0; // center free
        return grid;
    }

    private List<int> Generate90Numbers()
    {
        var grid = new List<int>(new int[15]);
        var colPositions = new int[5][];
        colPositions[0] = new int[] {0,5,10};
        colPositions[1] = new int[] {1,6,11};
        colPositions[2] = new int[] {2,7,12};
        colPositions[3] = new int[] {3,8,13};
        colPositions[4] = new int[] {4,9,14};
        var ranges = new (int, int)[] { (1,18), (19,36), (37,54), (55,72), (73,90) };

        for (int col = 0; col < 5; col++)
        {
            var nums = GenerateColumnNumbers(ranges[col].Item1, ranges[col].Item2, 3);
            nums = nums.OrderBy(x => _random.Next()).ToList();
            for (int i = 0; i < 3; i++)
            {
                int pos = colPositions[col][i];
                if (pos < 0 || pos >= grid.Count) throw new InvalidOperationException($"Invalid position {pos} for col {col}, i {i}");
                grid[pos] = nums[i];
            }
        }
        return grid;
    }

    private List<int> GenerateColumnNumbers(int min, int max, int count)
    {
        var available = Enumerable.Range(min, max - min + 1).ToList();
        var selected = available.OrderBy(x => _random.Next()).Take(count).ToList();
        return selected;
    }

    private Dictionary<string, bool> InitializeMarks(BingoType type)
    {
        var totalPositions = type == BingoType.SeventyFive ? 25 : 15;
        var marks = new Dictionary<string, bool>();
        for (int i = 0; i < totalPositions; i++)
        {
            marks[i.ToString()] = false;
        }
        if (type == BingoType.SeventyFive)
        {
            marks["12"] = true;
        }
        return marks;
    }

    public async Task<Guid> StartGameAsync(Guid roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null) throw new ArgumentException("Room not found");

        var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session == null)
        {
            session = new GameSession
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                DrawnBalls = new List<int>(),
                Status = GameStatus.Active,
                StartedAt = DateTime.UtcNow
            };
            _context.GameSessions.Add(session);
        }
        else if (session.Status != GameStatus.Active)
        {
            session.Status = GameStatus.Active;
            session.StartedAt = DateTime.UtcNow;
            session.DrawnBalls.Clear();
        }
        else
        {
            throw new InvalidOperationException("Game is already active");
        }

        await _context.SaveChangesAsync();
        return session.Id;
    }

    public async Task<int> DrawBallAsync(Guid roomId)
    {
        var session = await _context.GameSessions
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.RoomId == roomId && s.Status == GameStatus.Active);
        if (session == null) throw new InvalidOperationException("No active game session");

        var maxBall = session.Room.Type == BingoType.SeventyFive ? 75 : 90;
        var available = Enumerable.Range(1, maxBall).Where(b => !session.DrawnBalls.Contains(b)).ToList();
        if (available.Count == 0) throw new InvalidOperationException("No more balls to draw");

        var ball = available[_random.Next(available.Count)];
        session.DrawnBalls.Add(ball);
        await _context.SaveChangesAsync();
        return ball;
    }

    public async Task<List<int>> GetDrawnBallsAsync(Guid roomId)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session == null) return new List<int>();
        return session.DrawnBalls.OrderBy(b => b).ToList();
    }

    public bool IsHost(Guid userId, Guid roomId)
    {
        var room = _context.Rooms.Find(roomId);
        return room != null && room.HostId == userId;
    }

    public async Task<WinDto?> UpdateMarkAsync(Guid roomId, Guid cardId, string position, bool marked)
    {
        var card = await _context.BingoCards
            .Include(c => c.Player)
            .FirstOrDefaultAsync(c => c.Id == cardId && c.RoomId == roomId);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        if (!int.TryParse(position, out int posIndex) || posIndex < 0 || posIndex >= card.Numbers.Count)
        {
            throw new ArgumentException("Invalid position");
        }

        var num = card.Numbers[posIndex];

        var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session == null)
        {
            throw new InvalidOperationException("No game session");
        }

        var drawnBalls = session.DrawnBalls;

        if (marked && num > 0 && !drawnBalls.Contains(num))
        {
            throw new InvalidOperationException("Number not drawn yet");
        }

        card.Marks[position] = marked;
        await _context.SaveChangesAsync();

        var win = await CheckWinAsync(cardId, drawnBalls, useClientMarks: true);
        if (win != null)
        {
            session.WinnerId = card.PlayerId ?? Guid.Empty;
            session.Status = GameStatus.Ended;
            session.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return win;
    }

    public async Task<WinDto?> CheckWinAsync(Guid cardId, List<int> drawnBalls, bool useClientMarks = false)
    {
        var card = await _context.BingoCards
            .Include(c => c.Player)
            .FirstOrDefaultAsync(c => c.Id == cardId);
        if (card == null) return null;

        var effectiveMarks = new Dictionary<string, bool>();
        for (int i = 0; i < card.Numbers.Count; i++)
        {
            var pos = i.ToString();
            var num = card.Numbers[i];
            bool isMarked;
            if (useClientMarks)
            {
                isMarked = card.Marks.TryGetValue(pos, out var m) && m;
            }
            else
            {
                isMarked = (num == 0) || (num > 0 && drawnBalls.Contains(num));
            }
            effectiveMarks[pos] = isMarked;
        }

        if (card.Type == BingoType.Ninety)
        {
            var lines = new[] {
                new[] { "0", "1", "2", "3", "4" },
                new[] { "5", "6", "7", "8", "9" },
                new[] { "10", "11", "12", "13", "14" }
            };
            int completeLines = 0;
            List<int> completeIndices = new List<int>();
            for (int l = 0; l < 3; l++)
            {
                bool complete = true;
                foreach (var pos in lines[l])
                {
                    if (!effectiveMarks[pos])
                    {
                        complete = false;
                        break;
                    }
                }
                if (complete)
                {
                    completeLines++;
                    completeIndices.Add(l);
                }
            }
            if (completeLines == 3)
            {
                return new WinDto
                {
                    CardId = cardId,
                    PlayerId = card.PlayerId ?? Guid.Empty,
                    Pattern = "full_house",
                    IsFull = true
                };
            }
            if (completeLines == 2)
            {
                return new WinDto
                {
                    CardId = cardId,
                    PlayerId = card.PlayerId ?? Guid.Empty,
                    Pattern = "two_lines",
                    IsFull = false
                };
            }
            if (completeLines == 1)
            {
                var lineNum = completeIndices[0] + 1;
                return new WinDto
                {
                    CardId = cardId,
                    PlayerId = card.PlayerId ?? Guid.Empty,
                    Pattern = $"line{lineNum}",
                    IsFull = false
                };
            }
            return null;
        }
        else // SeventyFive
        {
            var patterns = GetWinPatterns();
            WinPattern? fullPattern = null;
            WinPattern? firstNonFull = null;
            foreach (var pattern in patterns)
            {
                bool allMarked = true;
                foreach (var pos in pattern.Positions)
                {
                    if (!effectiveMarks[pos])
                    {
                        allMarked = false;
                        break;
                    }
                }
                if (allMarked)
                {
                    if (pattern.IsFull)
                    {
                        fullPattern = pattern;
                    }
                    else if (firstNonFull == null)
                    {
                        firstNonFull = pattern;
                    }
                }
            }
            if (fullPattern != null)
            {
                return new WinDto
                {
                    CardId = cardId,
                    PlayerId = card.PlayerId ?? Guid.Empty,
                    Pattern = fullPattern.Name,
                    IsFull = true
                };
            }
            if (firstNonFull != null)
            {
                return new WinDto
                {
                    CardId = cardId,
                    PlayerId = card.PlayerId ?? Guid.Empty,
                    Pattern = firstNonFull.Name,
                    IsFull = false
                };
            }
            return null;
        }
    }

    private List<WinPattern> GetWinPatterns()
    {
        var patterns = new List<WinPattern>();
        // Rows
        patterns.Add(new WinPattern { Name = "row1", Positions = new[] { "0", "1", "2", "3", "4" } });
        patterns.Add(new WinPattern { Name = "row2", Positions = new[] { "5", "6", "7", "8", "9" } });
        patterns.Add(new WinPattern { Name = "row3", Positions = new[] { "10", "11", "12", "13", "14" } });
        patterns.Add(new WinPattern { Name = "row4", Positions = new[] { "15", "16", "17", "18", "19" } });
        patterns.Add(new WinPattern { Name = "row5", Positions = new[] { "20", "21", "22", "23", "24" } });
        // Columns
        patterns.Add(new WinPattern { Name = "col1", Positions = new[] { "0", "5", "10", "15", "20" } });
        patterns.Add(new WinPattern { Name = "col2", Positions = new[] { "1", "6", "11", "16", "21" } });
        patterns.Add(new WinPattern { Name = "col3", Positions = new[] { "2", "7", "12", "17", "22" } });
        patterns.Add(new WinPattern { Name = "col4", Positions = new[] { "3", "8", "13", "18", "23" } });
        patterns.Add(new WinPattern { Name = "col5", Positions = new[] { "4", "9", "14", "19", "24" } });
        // Diagonals
        patterns.Add(new WinPattern { Name = "diag1", Positions = new[] { "0", "6", "12", "18", "24" } });
        patterns.Add(new WinPattern { Name = "diag2", Positions = new[] { "4", "8", "12", "16", "20" } });
        // Full
        patterns.Add(new WinPattern { Name = "full", Positions = Enumerable.Range(0, 25).Select(i => i.ToString()).ToArray(), IsFull = true });
        return patterns;
    }

    private class WinPattern
    {
        public string Name { get; set; } = string.Empty;
        public string[] Positions { get; set; } = Array.Empty<string>();
        public bool IsFull { get; set; }
    }

    public async Task<WinDto?> CheckWinsAfterDrawAsync(Guid roomId)
    {
        var session = await _context.GameSessions
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.RoomId == roomId && s.Status == GameStatus.Active);
        if (session == null) return null;

        var cards = await _context.BingoCards
            .Include(c => c.Player)
            .Where(c => c.RoomId == roomId)
            .ToListAsync();

        WinDto? firstWin = null;
        foreach (var card in cards)
        {
            var win = await CheckWinAsync(card.Id, session.DrawnBalls, useClientMarks: false);
            if (win != null)
            {
                firstWin = win;
                break;
            }
        }

        if (firstWin != null)
        {
            session.WinnerId = firstWin.PlayerId;
            session.Status = GameStatus.Ended;
            session.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return firstWin;
    }
    public async Task PauseGameAsync(Guid roomId)
    {
        var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session == null || session.Status != GameStatus.Active) throw new InvalidOperationException("No active game to pause");
        session.Status = GameStatus.Paused;
        await _context.SaveChangesAsync();
    }

    public async Task EndGameAsync(Guid roomId)
    {
        var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.RoomId == roomId);
        if (session == null) throw new InvalidOperationException("No game session found");
        session.Status = GameStatus.Ended;
        if (session.WinnerId == Guid.Empty)
        {
            // Optionally set a default winner or leave as is
        }
        session.EndedAt ??= DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
