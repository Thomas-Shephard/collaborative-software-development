using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Models;
using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        public StudentDashboard()
        {
            InitializeComponent();
            
            // Get user roles from service and filter available roles
            var app = Application.Current as App;
            var userRoleService = app?.ServiceProvider?.GetService<IUserRoleService>();
            
            var viewModel = new StudentDashboardViewModel();
            
            if (userRoleService != null)
            {
                var availableDashboards = userRoleService.GetAvailableDashboards();
                viewModel.AvailableRoles = new ObservableCollection<string>(availableDashboards);
            }
            
            DataContext = viewModel;
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Tab selection handling - future functionality
            // The Student Dashboard uses a different layout structure
            // that doesn't require visibility toggling
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

    public class StudentDashboardViewModel : BaseViewModel
    {
        private int _selectedTabIndex = 0;
        private ObservableCollection<string> _availableRoles = new();

        public string StudentInitials
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "ST";

        public int UpcomingTests
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 3;

        public int CompletedTests
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 12;

        public double AverageScore
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 82.5;

        public string CurrentGrade
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "B+";

        public ObservableCollection<TestItem> UpcomingTestItems { get; set; }
        public ObservableCollection<TestItem> CompletedTestItems { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

        public ObservableCollection<string> AvailableRoles
        {
            get => _availableRoles;
            set
            {
                _availableRoles = value;
                OnPropertyChanged();
            }
        }

        public string SelectedRole
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Student";

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public StudentDashboardViewModel()
        {
            // Initialize with Student role by default
            _availableRoles = new ObservableCollection<string> { "Student" };

            UpcomingTestItems = new ObservableCollection<TestItem>
            {
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Mathematics Final Exam", 
                    SubjectName = "Advanced Calculus", 
                    DateInfo = "Due: Dec 15, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Chemistry Quiz", 
                    SubjectName = "Organic Chemistry", 
                    DateInfo = "Due: Dec 18, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Programming Assignment", 
                    SubjectName = "Data Structures", 
                    DateInfo = "Due: Dec 20, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                }
            };

            CompletedTestItems = new ObservableCollection<TestItem>
            {
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Statistics Midterm", 
                    SubjectName = "Applied Statistics", 
                    DateInfo = "Completed: Dec 1, 2024",
                    StatusOrScore = "92%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "History Essay", 
                    SubjectName = "World History", 
                    DateInfo = "Completed: Nov 28, 2024",
                    StatusOrScore = "85%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Physics Lab Report", 
                    SubjectName = "General Physics", 
                    DateInfo = "Completed: Nov 25, 2024",
                    StatusOrScore = "78%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "English Literature Quiz", 
                    SubjectName = "Modern Literature", 
                    DateInfo = "Completed: Nov 20, 2024",
                    StatusOrScore = "88%",
                    StatusLabel = "GRADE"
                }
            };

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Overview" },
                new TabItem { Header = "My Tests" },
                new TabItem { Header = "Performance" },
                new TabItem { Header = "Calendar" }
            };
        }
    }
}
