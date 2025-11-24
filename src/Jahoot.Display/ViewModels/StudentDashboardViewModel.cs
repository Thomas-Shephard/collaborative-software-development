using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Models;

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

    public class RecentActivityItem
    {
        public required string StudentInitials { get; set; }
        public required string DescriptionPrefix { get; set; }
        public required string TestName { get; set; }
        public required string TimeAgo { get; set; }
        public required string Result { get; set; }
    }

    public class PerformanceSubject
    {
        public required string SubjectName { get; set; }
        public required string ScoreText { get; set; }
        public required double ScoreValue { get; set; }
    }


    public class StudentDashboardViewModel : BaseViewModel
    {
        private int _selectedTabIndex = 0;
        private Visibility _overviewVisibility = Visibility.Visible;
        private Visibility _otherContentVisibility = Visibility.Collapsed;
        private object? _currentView;

        private string _selectedTab = "Available";

        public ObservableCollection<TestItem> UpcomingTests { get; } = new();
        public ObservableCollection<TestItem> CompletedTests { get; } = new();

        public ObservableCollection<string> TabItems { get; set; }
        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; } = new();
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; } = new();

        public StudentDashboardViewModel()
        {
            // Sample upcoming tests
            UpcomingTests.Add(new TestItem
            {
                Title = "Mathematics Midterm",
                Course = "Math 101",
                TestInfo = "20 questions • 60 mins",
                DueDate = DateTime.Today.AddDays(3),
                QuestionCount = 20
            });
            UpcomingTests.Add(new TestItem
            {
                Title = "History Chapter 5 Quiz",
                Course = "History 201",
                TestInfo = "15 questions • 30 mins",
                DueDate = DateTime.Today.AddDays(7),
                QuestionCount = 15
            });

            // Sample completed tests
            CompletedTests.Add(new TestItem
            {
                Title = "Intro to Algebra",
                Course = "Math 101",
                Date = DateTime.Today.AddDays(-14),
                Score = 78,
                QuestionCount = 10,
                PercentageCorrect = 78,
                TotalPoints = 210,
                TestInfo = "10 questions • Completed"
            });
            CompletedTests.Add(new TestItem
            {
                Title = "World War II",
                Course = "History 201",
                Date = DateTime.Today.AddDays(-10),
                Score = 85,
                QuestionCount = 12,
                PercentageCorrect = 85,
                TotalPoints = 270,
                TestInfo = "12 questions • Completed"
            });
            CompletedTests.Add(new TestItem
            {
                Title = "Calculus Basics",
                Course = "Math 101",
                Date = DateTime.Today.AddDays(-3),
                Score = 92,
                QuestionCount = 15,
                PercentageCorrect = 92,
                TotalPoints = 390,
                TestInfo = "15 questions • Completed"
            });
        }

        public int TestsAvailable => UpcomingTests.Count;
        public int TestsCompleted => CompletedTests.Count;
        public int UpcomingDueDates => UpcomingTests.Count(t => t.DueDate.HasValue && t.DueDate.Value <= DateTime.Today.AddDays(7));

        public double AverageScore
        {
            get
            {
                var scores = CompletedTests.Select(t => t.Score).Where(s => s.HasValue).Select(s => s!.Value).ToList();
                if (!scores.Any()) return 0;
                return Math.Round(scores.Average(), 1);
            }
        }

        public string AverageScoreDisplay => CompletedTests.Any() ? AverageScore.ToString("0.0") : "N/A";

        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
