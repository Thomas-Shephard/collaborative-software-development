using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Jahoot.Display.Models;
using Jahoot.Display.Controls;

namespace Jahoot.Display.ViewModels
{
    public class StudentDashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

        private int _selectedTabIndex = 0;

        public int SelectedTabIndex
    {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                    UpdateTabVisibility();
    }
            }
        }

        public string StudentName { get; set; } = "John Doe";
        public string StudentInitials { get; set; } = "JD";

        public ObservableCollection<TabItem> TabItems { get; set; }
        public ObservableCollection<TestItem> UpcomingTests { get; } = new();
        public ObservableCollection<TestItem> CompletedTests { get; } = new();

        public ObservableCollection<string> TabItems { get; set; }
        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; } = new();
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; } = new();
        public ObservableCollection<GradeTrendItem> GradeTrendItems { get; set; } = new();
        public ObservableCollection<LeaderboardEntry> LeaderboardEntries { get; set; } = new();

        public StudentDashboardViewModel()
        {
            TabItems = new ObservableCollection<string> { "Overview", "Available Tests", "Completed Tests", "Leaderboard", "Statistics" };
            UpdateVisibleContent();
        }

        public int TestsAvailable => UpcomingTests.Count;
        public int TestsCompleted => CompletedTests.Count;
        public int UpcomingDueDates => UpcomingTests.Count(t => t.DueDate.HasValue && t.DueDate.Value <= DateTime.Today.AddDays(7));

        public double AverageScore
        {
            get
            {
                var scores = CompletedTests.Where(t => t.Score.HasValue).Select(t => t.Score!.Value).ToList();
                if (!scores.Any()) return 0;
                return Math.Round(scores.Average(), 1);
            }
        }

        public string AverageScoreDisplay => CompletedTests.Any() ? $"{AverageScore:0.0}%" : "N/A";
        public string HighestScoreDisplay
        {
            get
            {
                var scores = CompletedTests.Where(t => t.Score.HasValue).Select(t => t.Score!.Value).ToList();
                if (!scores.Any()) return "N/A";
                return $"{scores.Max():0.0}%";
            }
        }

        private Visibility _overviewVisibility = Visibility.Visible;
        private Visibility _upcomingTestsVisibility = Visibility.Collapsed;
        private Visibility _completedTestsVisibility = Visibility.Collapsed;
        private Visibility _statisticsVisibility = Visibility.Collapsed;
        private Visibility _leaderboardVisibility = Visibility.Collapsed;

        public Visibility OverviewVisibility
        {
            get => _overviewVisibility;
            set
            {
                _overviewVisibility = value;
                    OnPropertyChanged();
                    UpdateVisibleContent();
                }
            }

        public Visibility UpcomingTestsVisibility
        {
            get => _upcomingTestsVisibility;
            set
            {
                _upcomingTestsVisibility = value;
                OnPropertyChanged();
        }
        }
        
        public Visibility CompletedTestsVisibility
        {
            get => _completedTestsVisibility;
            set
            {
                _completedTestsVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public Visibility StatisticsVisibility
        {
            get => _statisticsVisibility;
            set
            {
                _statisticsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility LeaderboardVisibility
        {
            get => _leaderboardVisibility;
            set
            {
                _leaderboardVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility NoUpcomingTestsVisibility => UpcomingTests.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NoCompletedTestsVisibility => CompletedTests.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public StudentDashboardViewModel()
        {
            TabItems = new ObservableCollection<TabItem>
        {
                new TabItem { Header = "Overview" },
                new TabItem { Header = "Upcoming Tests" },
                new TabItem { Header = "Completed Tests" },
                new TabItem { Header = "Leaderboard" },
                new TabItem { Header = "Statistics" }
            };

            LoadMockData();
            UpdateTabVisibility();
        }

        private void UpdateTabVisibility()
            {
            OverviewVisibility = SelectedTabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpcomingTestsVisibility = SelectedTabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            CompletedTestsVisibility = SelectedTabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
            LeaderboardVisibility = SelectedTabIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
            StatisticsVisibility = SelectedTabIndex == 4 ? Visibility.Visible : Visibility.Collapsed;
            }

        private void LoadMockData()
        {
            // Mock upcoming tests
            UpcomingTests.Add(new TestItem
            {
                Title = "Mathematics Final",
                Course = "MATH301",
                TestInfo = "20 questions • 60 mins",
                DueDate = DateTime.Today.AddDays(5),
                QuestionCount = 20
            });

            UpcomingTests.Add(new TestItem
            {
                Title = "Physics Quiz",
                Course = "PHYS201",
                TestInfo = "15 questions • 45 mins",
                DueDate = DateTime.Today.AddDays(10),
                QuestionCount = 15
            });

            // Mock completed tests
            CompletedTests.Add(new TestItem
            {
                Title = "English Literature Exam",
                Course = "ENG401",
                TestInfo = "25 questions • 75 mins",
                Date = DateTime.Today.AddDays(-5),
                Score = 88.5,
                QuestionCount = 25
            });

            CompletedTests.Add(new TestItem
                {
                Title = "Chemistry Midterm",
                Course = "CHEM202",
                TestInfo = "30 questions • 90 mins",
                Date = DateTime.Today.AddDays(-12),
                Score = 92.0,
                QuestionCount = 30
            });

            // Mock performance subjects
            PerformanceSubjects.Add(new PerformanceSubject { SubjectName = "Mathematics", ScoreText = "85%", ScoreValue = 85 });
            PerformanceSubjects.Add(new PerformanceSubject { SubjectName = "Physics", ScoreText = "78%", ScoreValue = 78 });
            PerformanceSubjects.Add(new PerformanceSubject { SubjectName = "Chemistry", ScoreText = "92%", ScoreValue = 92 });
            PerformanceSubjects.Add(new PerformanceSubject { SubjectName = "English", ScoreText = "88%", ScoreValue = 88 });

            // Mock grade trend
            GradeTrendItems.Add(new GradeTrendItem { TestName = "Math Quiz 1", Score = 85, ScoreText = "85%", ProgressBarColor = new SolidColorBrush(Color.FromRgb(5, 150, 105)) });
            GradeTrendItems.Add(new GradeTrendItem { TestName = "Physics Test", Score = 78, ScoreText = "78%", ProgressBarColor = new SolidColorBrush(Color.FromRgb(234, 179, 8)) });
            GradeTrendItems.Add(new GradeTrendItem { TestName = "Chem Midterm", Score = 92, ScoreText = "92%", ProgressBarColor = new SolidColorBrush(Color.FromRgb(5, 150, 105)) });

            // Mock leaderboard
            LeaderboardEntries.Add(new LeaderboardEntry { Rank = 1, StudentName = "Alice Johnson", StudentInitials = "AJ", Score = 95.5, ScoreText = "95.5%", TestsCompleted = 12, IsCurrentUser = false });
            LeaderboardEntries.Add(new LeaderboardEntry { Rank = 2, StudentName = "Bob Smith", StudentInitials = "BS", Score = 92.3, ScoreText = "92.3%", TestsCompleted = 11, IsCurrentUser = false });
            LeaderboardEntries.Add(new LeaderboardEntry { Rank = 3, StudentName = "John Doe", StudentInitials = "JD", Score = 88.7, ScoreText = "88.7%", TestsCompleted = 10, IsCurrentUser = true });
            LeaderboardEntries.Add(new LeaderboardEntry { Rank = 4, StudentName = "Carol White", StudentInitials = "CW", Score = 85.2, ScoreText = "85.2%", TestsCompleted = 9, IsCurrentUser = false });
                }
            }

    public class PerformanceSubject
    {
        public required string SubjectName { get; set; }
        public required string ScoreText { get; set; }
        public required double ScoreValue { get; set; }
        }

    public class GradeTrendItem
    {
        public required string TestName { get; set; }
        public required double Score { get; set; }
        public required string ScoreText { get; set; }
        public required SolidColorBrush ProgressBarColor { get; set; }
    }
}
