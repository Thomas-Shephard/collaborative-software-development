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
    public abstract class BaseViewModel : INotifyPropertyChanged
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
                var scores = CompletedTests.Select(t => t.Score).Where(s => s.HasValue).Select(s => s!.Value).ToList();
                if (!scores.Any()) return 0;
                return Math.Round(scores.Average(), 1);
            }
        }

        public string AverageScoreDisplay => CompletedTests.Any() ? AverageScore.ToString("0.0") : "N/A";

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                    UpdateVisibleContent();
                }
            }
        }
        
        public Visibility OverviewVisibility
        {
            get => _overviewVisibility;
            set
            {
                _overviewVisibility = value;
                OnPropertyChanged();
            }
        }
        
        public Visibility OtherContentVisibility
        {
            get => _otherContentVisibility;
            set
                {
                _otherContentVisibility = value;
                    OnPropertyChanged();
                }
        }

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private void UpdateVisibleContent()
        {
            if (SelectedTabIndex == 0) // Overview
            {
                OverviewVisibility = Visibility.Visible;
                OtherContentVisibility = Visibility.Collapsed;
            }
            else
            {
                OverviewVisibility = Visibility.Collapsed;
                OtherContentVisibility = Visibility.Visible;

                switch (TabItems[SelectedTabIndex])
        {
                    case "Available Tests":
                        CurrentView = new ItemsControl { ItemsSource = UpcomingTests, ItemTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("AvailableTestTemplate") };
                        break;
                    case "Completed Tests":
                        CurrentView = new ItemsControl { ItemsSource = CompletedTests, ItemTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("CompletedTestTemplate") };
                        break;
                    case "Leaderboard":
                        CurrentView = new TextBlock { Text = "Leaderboard coming soon...", Margin = new Thickness(20) };
                        break;
                    case "Statistics":
                        CurrentView = new TextBlock { Text = "Statistics coming soon...", Margin = new Thickness(20) };
                        break;
                }
            }
        }
    }
}
