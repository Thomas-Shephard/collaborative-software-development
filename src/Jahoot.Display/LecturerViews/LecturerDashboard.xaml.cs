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
        private readonly LecturerDashboardViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public LecturerDashboard(LecturerDashboardViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
            DataContext = _viewModel;
            Loaded += LecturerDashboard_Loaded;
        }

        private async void LecturerDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitialiseAsync();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LecturerDashboardViewModel viewModel &&
                sender is NavigationalTabs tabs &&
                tabs.SelectedItem is NavigationTabItem selectedTab)
            {
                Type viewType = selectedTab.ViewType;

                UserControl? view = _serviceProvider.GetService(viewType) as UserControl;
                if (view == null) return;
                

                Type? viewModelType = null;
                if (viewType == typeof(LecturerOverviewView))
                {
    
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


                if (viewModelType != null)
                {
                    view.DataContext = _serviceProvider.GetRequiredService(viewModelType);
                }
                else
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
        private readonly IStudentService _studentService;
        private readonly ITestService _testService;
        private readonly IServiceProvider _serviceProvider; // Inject IServiceProvider
        private object _currentView = null!;
        private ObservableCollection<string> _availableRoles = new();
        
        private int _totalStudents;
        private int _activeTests;
        private double _averageScore;
        private double _completionRate;

        public string LecturerInitials { get; set; } = "JD";
        public int TotalStudents
        {
            get => _totalStudents;
            set
            {
                _totalStudents = value;
                OnPropertyChanged();
            }
        }
        public int ActiveTests
        {
            get => _activeTests;
            set
            {
                _activeTests = value;
                OnPropertyChanged();
            }
        }
        public double AverageScore
        {
            get => _averageScore;
            set
            {
                _averageScore = value;
                OnPropertyChanged();
            }
        }
        public double CompletionRate
        {
            get => _completionRate;
            set
            {
                _completionRate = value;
                OnPropertyChanged();
            }
        }
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

        public LecturerDashboardViewModel(IStudentService studentService, ITestService testService, IServiceProvider serviceProvider)
        {
            _studentService = studentService;
            _testService = testService;
            _serviceProvider = serviceProvider; // Assign injected service provider

            _availableRoles = new ObservableCollection<string> { "Lecturer" };
            
            RecentActivityItems = new ObservableCollection<RecentActivityItem>();
            PerformanceSubjects = new ObservableCollection<PerformanceSubject>();

            TabItems = new ObservableCollection<NavigationTabItem>
            {
                new NavigationTabItem { Header = "Overview", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Students", ViewType = typeof(StudentManagementView) },
                new NavigationTabItem { Header = "Tests", ViewType = typeof(TestManagementView) },
                new NavigationTabItem { Header = "Progress", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Leaderboard", ViewType = typeof(LecturerOverviewView) }
            };

            // Instantiate view and set DataContext using injected IServiceProvider
            var initialView = _serviceProvider.GetRequiredService(typeof(LecturerOverviewView)) as UserControl;
            if (initialView != null)
            {
                initialView.DataContext = _serviceProvider.GetRequiredService(typeof(LecturerOverviewViewModel));
                CurrentView = initialView;
            }
        }

        public async Task InitialiseAsync()
        {
            try
            {
                var approvedStudentsTask = _studentService.GetStudents(true);
                var unapprovedStudentsTask = _studentService.GetStudents(false);
                var testsTask = _testService.GetTests();

                await Task.WhenAll(approvedStudentsTask, unapprovedStudentsTask, testsTask);

                TotalStudents = approvedStudentsTask.Result.Count() + unapprovedStudentsTask.Result.Count();
                ActiveTests = testsTask.Result.Count();

                // Placeholder for Average Score and Completion Rate calculation
                AverageScore = 0; 
                CompletionRate = 0; 
                MessageBox.Show("No direct API endpoints for overall Average Score and Completion Rate. These are placeholders.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during dashboard initialization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
