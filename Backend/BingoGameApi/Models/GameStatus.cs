using System.ComponentModel.DataAnnotations;

namespace BingoGameApi.Models;

public enum GameStatus
{
    Waiting,
    Active,
    Paused,
    Ended
}