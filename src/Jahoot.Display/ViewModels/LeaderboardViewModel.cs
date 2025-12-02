using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Jahoot.Display.Models;

namespace Jahoot.Display.ViewModels;

public class LeaderboardViewModel : INotifyPropertyChanged
{
    private string _selectedSubject = "All Subjects";
    private ObservableCollection<LeaderboardEntry> _leaderboardEntries = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ObservableCollection<string> Subjects { get; set; }

    public string SelectedSubject
    {
        get => _selectedSubject;
        set
        {
            if (_selectedSubject != value)
            {
                _selectedSubject = value;
                OnPropertyChanged();
                LoadLeaderboardData();
            }
        }
    }

    public ObservableCollection<LeaderboardEntry> LeaderboardEntries
    {
        get => _leaderboardEntries;
        set
        {
            _leaderboardEntries = value;
            OnPropertyChanged();
        }
    }

    public LeaderboardViewModel()
    {
        // Initialize subjects list
        Subjects = new ObservableCollection<string>
        {
            "All Subjects",
            "Mathematics",
            "Science",
            "History",
            "English",
            "Computer Science",
            "Physics"
        };

        LoadLeaderboardData();
    }

    private void LoadLeaderboardData()
    {
        // Mock data for testing - will be replaced with API calls
        var mockData = SelectedSubject switch
        {
            "Mathematics" => new[]
            {
                new { Name = "Sarah Adams", Score = 95.2 },
                new { Name = "Emma Wilson", Score = 91.8 },
                new { Name = "David Johnson", Score = 89.4 },
                new { Name = "Mike Johnson", Score = 87.2 },
                new { Name = "Lisa Brown", Score = 85.9 }
            },
            "Physics" => new[]
            {
                new { Name = "Emma Wilson", Score = 93.7 },
                new { Name = "David Johnson", Score = 88.9 },
                new { Name = "Sarah Adams", Score = 86.3 },
                new { Name = "Alex Lee", Score = 84.1 },
                new { Name = "Mike Johnson", Score = 82.7 }
            },
            "Science" => new[]
            {
                new { Name = "David Johnson", Score = 94.5 },
                new { Name = "Sarah Adams", Score = 92.1 },
                new { Name = "Emma Wilson", Score = 88.6 },
                new { Name = "Alex Lee", Score = 85.3 },
                new { Name = "Lisa Brown", Score = 83.8 }
            },
            "History" => new[]
            {
                new { Name = "Lisa Brown", Score = 96.2 },
                new { Name = "Sarah Adams", Score = 91.7 },
                new { Name = "Mike Johnson", Score = 89.3 },
                new { Name = "Emma Wilson", Score = 87.5 },
                new { Name = "David Johnson", Score = 84.9 }
            },
            "English" => new[]
            {
                new { Name = "Emma Wilson", Score = 97.1 },
                new { Name = "Lisa Brown", Score = 93.4 },
                new { Name = "Sarah Adams", Score = 90.8 },
                new { Name = "Alex Lee", Score = 88.2 },
                new { Name = "Mike Johnson", Score = 86.5 }
            },
            "Computer Science" => new[]
            {
                new { Name = "Alex Lee", Score = 98.3 },
                new { Name = "David Johnson", Score = 95.7 },
                new { Name = "Mike Johnson", Score = 92.4 },
                new { Name = "Emma Wilson", Score = 89.1 },
                new { Name = "Sarah Adams", Score = 87.6 }
            },
            "All Subjects" => new[]
            {
                new { Name = "Sarah Adams", Score = 91.2 },
                new { Name = "Emma Wilson", Score = 89.8 },
                new { Name = "David Johnson", Score = 87.5 },
                new { Name = "Mike Johnson", Score = 85.3 },
                new { Name = "Lisa Brown", Score = 83.7 }
            },
            _ => System.Array.Empty<dynamic>()
        };

        var entries = mockData.Select((data, index) => new LeaderboardEntry
        {
            Rank = index + 1,
            StudentName = data.Name,
            StudentInitials = GetStudentInitials(data.Name),
            Score = data.Score,
            ScoreText = $"{data.Score:0.0}%",
            TestsCompleted = 10 + index,
            IsCurrentUser = false,
            RankIcon = GetRankIcon(index + 1)
        });

        LeaderboardEntries = new ObservableCollection<LeaderboardEntry>(entries);
    }

    private string? GetRankIcon(int rank)
    {
        return rank switch
        {
            1 => "🥇",  // Gold medal
            2 => "🥈",  // Silver medal
            3 => "🥉",  // Bronze medal
            _ => null
        };
    }

    private string GetStudentInitials(string fullName)
    {
        var nameParts = fullName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length >= 2)
        {
            return $"{nameParts[0][0]}{nameParts[^1][0]}".ToUpper();
        }
        else if (nameParts.Length == 1 && nameParts[0].Length >= 2)
        {
            return nameParts[0].Substring(0, 2).ToUpper();
        }
        return nameParts.Length > 0 ? nameParts[0][0].ToString().ToUpper() : "??";
    }

    // Method to populate leaderboard data (to be called when API integration is ready)
    public void UpdateLeaderboard(System.Collections.Generic.IEnumerable<LeaderboardEntry> entries)
    {
        LeaderboardEntries.Clear();
        
        foreach (var entry in entries.Take(5))
        {
            entry.RankIcon = GetRankIcon(entry.Rank);
            
            // Generate initials if not provided
            if (string.IsNullOrEmpty(entry.StudentInitials))
            {
                entry.StudentInitials = GetStudentInitials(entry.StudentName);
            }
            
            LeaderboardEntries.Add(entry);
        }
    }
}
