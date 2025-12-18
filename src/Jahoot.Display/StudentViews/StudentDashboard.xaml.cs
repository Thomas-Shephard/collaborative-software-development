using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Jahoot.Display.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;
using Jahoot.Display.Commands;
using Jahoot.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        public StudentDashboard()
        {
            InitializeComponent();
            
            var app = Application.Current as App;
            var userRoleService = app?.ServiceProvider?.GetService<IUserRoleService>();
            
            var viewModel = new StudentDashboardViewModel();
            
            if (userRoleService != null)
            {
                var availableDashboards = userRoleService.GetAvailableDashboards();
                viewModel.AvailableRoles = new ObservableCollection<string>(availableDashboards);
            }
            
            viewModel.TestItemClickCommand = new RelayCommand(OnTestItemClick, CanClickTestItem);
            
            DataContext = viewModel;
        }

        private bool CanClickTestItem(object? parameter)
        {
            return parameter is TestItem;
        }

        private void OnTestItemClick(object? parameter)
        {
            if (parameter is not TestItem testItem)
            {
                return;
            }

            if (testItem.StatusLabel == "NOT TAKEN")
            {
                try
                {
                    var app = Application.Current as App;
                    var testPage = app?.ServiceProvider?.GetRequiredService<TestTakingPage>();
                    
                    if (testPage != null && testPage.DataContext is TestTakingViewModel viewModel)
                    {
                        viewModel.LoadMockTest(testItem.TestId, testItem.TestName, testItem.SubjectName);
                        testPage.Show();
                    }
                    else
                    {
                        MessageBox.Show("Unable to open test page.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error opening test.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("This test has already been completed.", "Test Completed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is StudentDashboardViewModel viewModel)
            {
                viewModel.UpdateTabVisibility(viewModel.SelectedTabIndex);
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

    public class StudentDashboardViewModel : BaseViewModel
    {
        private int _selectedTabIndex = 0;
        private ObservableCollection<string> _availableRoles = new();
        private Visibility _availableTestsVisibility = Visibility.Visible;
        private Visibility _completedTestsVisibility = Visibility.Collapsed;
        private Visibility _leaderboardVisibility = Visibility.Collapsed;
        private Visibility _statisticsVisibility = Visibility.Collapsed;
        private static readonly DateTime _baseDate = new DateTime(2024, 11, 1);

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
        } = 4;

        public double AverageScore
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 85.75;

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
        public ObservableCollection<GradeDataPoint> GradeHistory { get; set; }
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
                UpdateTabVisibility(value);
            }
        }

        public Visibility AvailableTestsVisibility
        {
            get => _availableTestsVisibility;
            set
            {
                _availableTestsVisibility = value;
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

        public Visibility LeaderboardVisibility
        {
            get => _leaderboardVisibility;
            set
            {
                _leaderboardVisibility = value;
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

        public ICommand? TestItemClickCommand { get; set; }

        public StudentDashboardViewModel()
        {
            _availableRoles = new ObservableCollection<string> { "Student" };

            UpcomingTestItems = new ObservableCollection<TestItem>
            {
                new TestItem 
                { 
                    TestId = 1,
                    Icon = "??", 
                    TestName = "Mathematics Final Exam", 
                    SubjectName = "Advanced Calculus", 
                    DateInfo = "Due: Dec 15, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                },
                new TestItem 
                { 
                    TestId = 2,
                    Icon = "??", 
                    TestName = "Chemistry Quiz", 
                    SubjectName = "Organic Chemistry", 
                    DateInfo = "Due: Dec 18, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                },
                new TestItem 
                { 
                    TestId = 3,
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
                    TestId = 4,
                    Icon = "??", 
                    TestName = "Statistics Midterm", 
                    SubjectName = "Applied Statistics", 
                    DateInfo = "Completed: Dec 1, 2024",
                    StatusOrScore = "92%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    TestId = 5,
                    Icon = "??", 
                    TestName = "History Essay", 
                    SubjectName = "World History", 
                    DateInfo = "Completed: Nov 28, 2024",
                    StatusOrScore = "85%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    TestId = 6,
                    Icon = "??", 
                    TestName = "Physics Lab Report", 
                    SubjectName = "General Physics", 
                    DateInfo = "Completed: Nov 25, 2024",
                    StatusOrScore = "78%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    TestId = 7,
                    Icon = "??", 
                    TestName = "English Literature Quiz", 
                    SubjectName = "Modern Literature", 
                    DateInfo = "Completed: Nov 20, 2024",
                    StatusOrScore = "88%",
                    StatusLabel = "GRADE"
                }
            };

            GradeHistory = new ObservableCollection<GradeDataPoint>
            {
                new GradeDataPoint { TestName = "History Essay", Score = 85, TestDate = _baseDate.AddDays(20) },
                new GradeDataPoint { TestName = "Physics Lab", Score = 78, TestDate = _baseDate.AddDays(25) },
                new GradeDataPoint { TestName = "Literature Quiz", Score = 88, TestDate = _baseDate.AddDays(28) },
                new GradeDataPoint { TestName = "Statistics Midterm", Score = 92, TestDate = _baseDate.AddDays(30) }
            };

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Available" },
                new TabItem { Header = "Completed" },
                new TabItem { Header = "Leaderboard" },
                new TabItem { Header = "Statistics" }
            };
        }

        public void UpdateTabVisibility(int tabIndex)
        {
            AvailableTestsVisibility = Visibility.Collapsed;
            CompletedTestsVisibility = Visibility.Collapsed;
            LeaderboardVisibility = Visibility.Collapsed;
            StatisticsVisibility = Visibility.Collapsed;

            switch (tabIndex)
            {
                case 0:
                    AvailableTestsVisibility = Visibility.Visible;
                    break;
                case 1:
                    CompletedTestsVisibility = Visibility.Visible;
                    break;
                case 2:
                    LeaderboardVisibility = Visibility.Visible;
                    break;
                case 3:
                    StatisticsVisibility = Visibility.Visible;
                    break;
            }
        }
    }
}
