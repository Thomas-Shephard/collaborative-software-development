using System;

namespace Jahoot.Display.Models;

public class LeaderboardEntry
{
    public required int Rank { get; set; }
    public required string StudentName { get; set; }
    public required string StudentInitials { get; set; }
    public required double Score { get; set; }
    public required string ScoreText { get; set; }
    public required int TestsCompleted { get; set; }
    public required bool IsCurrentUser { get; set; }
    public string? RankIcon { get; set; }
    
    public string RankDisplay
    {
        get
        {
            int rem100 = Rank % 100;
            if (rem100 == 11 || rem100 == 12 || rem100 == 13)
                return $"{Rank}th";
            switch (Rank % 10)
            {
                case 1: return $"{Rank}st";
                case 2: return $"{Rank}nd";
                case 3: return $"{Rank}rd";
                default: return $"{Rank}th";
            }
        }
    }
}
