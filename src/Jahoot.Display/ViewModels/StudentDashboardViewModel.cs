using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public ObservableCollection<TestItem> UpcomingTests { get; } = new();
        public ObservableCollection<TestItem> CompletedTests { get; } = new();

        public ObservableCollection<string> TabItems { get; set; }
        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; } = new();
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; } = new();

        public StudentDashboardViewModel()
        {
            // Sample data for demo / tests
            UpcomingTests.Add(new TestItem{ Title = "Math Midterm", Date = DateTime.Today.AddDays(3), Course = "Math 101" });
            UpcomingTests.Add(new TestItem{ Title = "History Quiz", Date = DateTime.Today.AddDays(7), Course = "History 201" });

            CompletedTests.Add(new TestItem{ Title = "Intro Quiz", Date = DateTime.Today.AddDays(-14), Score = 78, Course = "Math 101" });
            CompletedTests.Add(new TestItem{ Title = "Chapter 1 Test", Date = DateTime.Today.AddDays(-10), Score = 85, Course = "History 201" });
            CompletedTests.Add(new TestItem{ Title = "Pop Quiz", Date = DateTime.Today.AddDays(-3), Score = 92, Course = "Math 101" });

        }

        public double AverageScore
        {
            get
            {
                var scores = CompletedTests.Select(t => t.Score).Where(s => s.HasValue).Select(s => s!.Value).ToList();
                if (!scores.Any()) return 0;
                return Math.Round(scores.Average(), 1);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
