using Jahoot.Display.Services;
using Jahoot.Core.Models;
using Jahoot.Display.Controls;
using Jahoot.Display.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Models;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        public LecturerDashboard()
        {
            InitializeComponent();
            
            var app = Application.Current as App;
            var userRoleService = app?.ServiceProvider?.GetService<IUserRoleService>();
            
            var viewModel = new LecturerDashboardViewModel();
            
            if (userRoleService != null)
            {
                var availableDashboards = userRoleService.GetAvailableDashboards();
                viewModel.AvailableRoles = new ObservableCollection<string>(availableDashboards);
            }
            
            DataContext = viewModel;
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LecturerDashboardViewModel viewModel &&
                sender is NavigationalTabs tabs &&
                tabs.SelectedItem is NavigationTabItem selectedTab)
            {
                Type viewType = selectedTab.ViewType;

                UserControl? view = Activator.CreateInstance(viewType) as UserControl;
                if (view == null) return;
                
                // Determine the corresponding ViewModel type
                Type? viewModelType = null;
                if (viewType == typeof(LecturerOverviewView))
                {
                    // Assuming LecturerOverviewViewModel is created and registered
                    viewModelType = typeof(LecturerOverviewViewModel);
                }
                else if (viewType == typeof(StudentManagementView))
                {
                    viewModelType = typeof(StudentManagementViewModel);
                }
                else if (viewType == typeof(TestManagementView))
                {
                    viewModelType = typeof(TestManagementViewModel);
                }
                // Add more conditions for other views as needed

                if (viewModelType != null)
                {
                    view.DataContext = ((App)Application.Current).ServiceProvider.GetRequiredService(viewModelType);
                }
                else
                {
                    // Fallback or error handling if no ViewModel is found for the ViewType
                    // For now, keep existing behavior for unmapped views, but this should ideally not happen
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
        private ObservableCollection<string> _availableRoles = new();
        
        public string LecturerInitials { get; set; } = "JD";
        public int TotalStudents { get; set; } = 120;
        public int ActiveTests { get; set; } = 5;
        public double AverageScore { get; set; } = 78.5;
        public double CompletionRate { get; set; } = 85;
        public string HeaderDescription { get; } = "Welcome to your lecturer dashboard.";

        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }
        public ObservableCollection<NavigationTabItem> TabItems { get; set; }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set
            {
                _availableRoles = value;
                OnPropertyChanged();
            }
        }
        
        public string SelectedRole { get; set; } = "Lecturer";
        public int SelectedTabIndex { get; set; } = 0;

        public LecturerDashboardViewModel()
        {
            _availableRoles = new ObservableCollection<string> { "Lecturer" };
            
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
                new NavigationTabItem { Header = "Tests", ViewType = typeof(TestManagementView) },
                new NavigationTabItem { Header = "Progress", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Leaderboard", ViewType = typeof(LecturerOverviewView) }
            };

            CurrentView = new LecturerOverviewView { DataContext = this };
        }
    }
}
