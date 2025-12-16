using Jahoot.Core.Models;
using Jahoot.Display.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        public LecturerDashboard()
        {
            InitializeComponent();
            this.DataContext = new LecturerDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LecturerDashboardViewModel viewModel &&
                sender is NavigationalTabs tabs &&
                tabs.SelectedItem is NavigationTabItem selectedTab)
            {
                Type viewType = selectedTab.ViewType;

                UserControl? view = Activator.CreateInstance(viewType) as UserControl;
                if (view == null) return;

                if (view is not StudentManagementView)
                {
                    view.DataContext = viewModel;
                }
                
                viewModel.CurrentView = view;
            }
        }
    }

    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LecturerDashboardViewModel : BaseViewModel
    {
        private object _currentView = null!;
        public string LecturerInitials { get; set; } = "JD";
        public int TotalStudents { get; set; } = 120;
        public int ActiveTests { get; set; } = 5;
        public double AverageScore { get; set; } = 78.5;
        public double CompletionRate { get; set; } = 85;

        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }
        public ObservableCollection<NavigationTabItem> TabItems { get; set; }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AvailableRoles { get; set; } = new ObservableCollection<string> { "Student", "Lecturer", "Admin" };
        public string SelectedRole { get; set; } = "Lecturer";
        public int SelectedTabIndex { get; set; } = 0;

        public LecturerDashboardViewModel()
        {
            RecentActivityItems = new ObservableCollection<RecentActivityItem>
            {
                new RecentActivityItem { StudentInitials = "AS", DescriptionPrefix = "Student ", TestName = "Math Quiz", TimeAgo = "5 mins ago", Result = "100%" },
                new RecentActivityItem { StudentInitials = "BM", DescriptionPrefix = "Student ", TestName = "Science Test", TimeAgo = "1 hour ago", Result = "85%" },
                new RecentActivityItem { StudentInitials = "CJ", DescriptionPrefix = "Student ", TestName = "History Exam", TimeAgo = "2 hours ago", Result = "72%" }
            };

            PerformanceSubjects = new ObservableCollection<PerformanceSubject>
            {
                new PerformanceSubject { SubjectName = "Mathematics", ScoreText = "88%", ScoreValue = 88 },
                new PerformanceSubject { SubjectName = "Science", ScoreText = "75%", ScoreValue = 75 },
                new PerformanceSubject { SubjectName = "History", ScoreText = "60%", ScoreValue = 60 },
                new PerformanceSubject { SubjectName = "English", ScoreText = "92%", ScoreValue = 92 }
            };

            TabItems = new ObservableCollection<NavigationTabItem>
            {
                new NavigationTabItem { Header = "Overview", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Students", ViewType = typeof(StudentManagementView) },
                new NavigationTabItem { Header = "Tests", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Progress", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Leaderboard", ViewType = typeof(LecturerOverviewView) }
            };

            CurrentView = new LecturerOverviewView { DataContext = this };
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

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}