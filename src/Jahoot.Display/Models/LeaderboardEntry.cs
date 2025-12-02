using System;

namespace Jahoot.Display.Models;

public class LeaderboardEntry
{
    public required int Rank { get; set; }
    public required string StudentName { get; set; }
    public required string StudentInitials { get; set; }
    public required double Score { get; set; }
    public string? RankIcon { get; set; }
    
    public string RankDisplay => Rank switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ => $"{Rank}th"
    };
}
