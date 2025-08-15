
using System;

namespace HuaCards.Models;

public class ReviewLog
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public Card Card { get; set; } = default!;
    public DateTime ReviewedAtUtc { get; set; }
    public int Grade { get; set; } // 4=Known, 2=Unknown
    public int PrevInterval { get; set; }
    public int NextInterval { get; set; }
    public double PrevEase { get; set; }
    public double NextEase { get; set; }
    public int ElapsedDays { get; set; }
}
