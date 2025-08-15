
using System;
using System.Collections.Generic;

namespace HuaCards.Models;

public class Word
{
    public int Id { get; set; }
    public string English { get; set; } = string.Empty;
    public string Hanzi { get; set; } = string.Empty;
    public string Pinyin { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<Card> Cards { get; set; } = new();
}
