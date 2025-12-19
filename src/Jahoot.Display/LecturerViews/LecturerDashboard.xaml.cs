using Jahoot.Display.Services;
using Jahoot.Core.Models;
using Jahoot.Display.Controls;
using Jahoot.Display.Commands;
using Jahoot.Display.AdminViews;
using Jahoot.Display.StudentViews;
using Jahoot.Display.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Models;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Jahoot.WebApi.Models.Responses;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        private readonly LecturerDashboardViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISecureStorageService _secureStorageService;

        public LecturerDashboard(LecturerDashboardViewModel viewModel, IServiceProvider serviceProvider, ISecureStorageService secureStorageService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
            _secureStorageService = secureStorageService;
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
                    viewModelType = typeof(LecturerOverviewViewModel);
                else if (viewType == typeof(StudentManagementView))
                    viewModelType = typeof(StudentManagementViewModel);
                else if (viewType == typeof(TestManagementView))
                    viewModelType = typeof(TestManagementViewModel);

                view.DataContext = viewModelType != null
                    ? _serviceProvider.GetRequiredService(viewModelType)
                    : viewModel;

                if (view is not StudentManagementView && view is not AssignStudentsToSubjectsView)
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ISecureStorageService _secureStorageService;
        private object _currentView = null!;
        private ObservableCollection<string> _availableRoles = new();
        private int _totalStudents;
        private int _totalTests;
        private int _pendingStudentApprovals;
        private int _totalTestAttempts;
        private string _selectedRole = "Lecturer";

        public string LecturerInitials { get; set; } = "JD";
        public int TotalStudents { get => _totalStudents; set { _totalStudents = value; OnPropertyChanged(); } }
        public int TotalTests { get => _totalTests; set { _totalTests = value; OnPropertyChanged(); } }
        public int PendingStudentApprovals { get => _pendingStudentApprovals; set { _pendingStudentApprovals = value; OnPropertyChanged(); } }
        public int TotalTestAttempts { get => _totalTestAttempts; set { _totalTestAttempts = value; OnPropertyChanged(); } }
        public string HeaderDescription { get; } = "Welcome to your lecturer dashboard.";
        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }
        public ObservableCollection<NavigationTabItem> TabItems { get; set; }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set { _availableRoles = value; OnPropertyChanged(); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (_selectedRole != value)
                {
                    _selectedRole = value;
                    OnPropertyChanged();
                }
            }
        }
        public int SelectedTabIndex { get; set; } = 0;

        public LecturerDashboardViewModel(IStudentService studentService, ITestService testService, IServiceProvider serviceProvider, ISecureStorageService secureStorageService)
        {
            _studentService = studentService;
            _testService = testService;
            _serviceProvider = serviceProvider;
            _secureStorageService = secureStorageService;
            RecentActivityItems = new ObservableCollection<RecentActivityItem>();
            PerformanceSubjects = new ObservableCollection<PerformanceSubject>();
            TabItems = new ObservableCollection<NavigationTabItem>
            {
                new NavigationTabItem { Header = "Overview", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Students", ViewType = typeof(StudentManagementView) },
                new NavigationTabItem { Header = "Assign Students to Subjects", ViewType = typeof(AssignStudentsToSubjectsView) },
                new NavigationTabItem { Header = "Tests", ViewType = typeof(TestManagementView) }
            };

            var initialView = _serviceProvider.GetRequiredService(typeof(LecturerOverviewView)) as UserControl;
            if (initialView != null)
            {
                initialView.DataContext = _serviceProvider.GetRequiredService(typeof(LecturerOverviewViewModel));
                CurrentView = initialView;
            }
        }

        public async Task InitialiseAsync()
        {
            var token = _secureStorageService.GetToken();
            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                AvailableRoles = new ObservableCollection<string>(roles);
            }

            IEnumerable<Student> approvedStudents = new List<Student>();
            IEnumerable<Student> unapprovedStudents = new List<Student>();
            IEnumerable<Test> tests = new List<Test>();
            IEnumerable<Jahoot.WebApi.Models.Responses.CompletedTestResponse> recentTests = new List<Jahoot.WebApi.Models.Responses.CompletedTestResponse>();

            try { approvedStudents = await _studentService.GetStudents(true); }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show($"Access Denied to Approved Students. {ex.Message}", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching approved students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try { unapprovedStudents = await _studentService.GetStudents(false); }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show($"Access Denied to Unapproved Students. {ex.Message}", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching unapproved students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try { tests = await _testService.GetTests(); }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show($"Access Denied to Tests. {ex.Message}", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try { recentTests = await _testService.GetRecentCompletedTests(365); }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show($"Access Denied to Recent Tests. {ex.Message}", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching recent completed tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TotalStudents = approvedStudents.Count() + unapprovedStudents.Count();
            PendingStudentApprovals = unapprovedStudents.Count();
            TotalTests = tests.Count();
            TotalTestAttempts = recentTests.Count();
        }
    }
}
