using System.Collections.Generic;

namespace BingoGameApi.DTOs;

public class DrawnBallsDto
{
    public List<int> DrawnBalls { get; set; } = new List<int>();
}