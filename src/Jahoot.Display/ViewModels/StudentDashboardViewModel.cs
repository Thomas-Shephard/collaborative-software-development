using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Jahoot.Display.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews;

public class StudentDashboardViewModel : BaseViewModel
{
    private readonly ITestService _testService;
    private int _selectedTabIndex = 0;
    private ObservableCollection<string> _availableRoles = new();
    private Visibility _availableTestsVisibility = Visibility.Visible;
    private Visibility _completedTestsVisibility = Visibility.Collapsed;
    private Visibility _leaderboardVisibility = Visibility.Collapsed;
    private Visibility _statisticsVisibility = Visibility.Collapsed;
    private bool _isLoading = false;
    private bool _isShowingTest = false;
    private object? _currentView = null;
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
    } = 0;

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

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool IsShowingTest
    {
        get => _isShowingTest;
        set
        {
            _isShowingTest = value;
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

    public ObservableCollection<TestItem> UpcomingTestItems { get; set; }
    public ObservableCollection<TestItem> CompletedTestItems { get; set; }
    public ObservableCollection<GradeDataPoint> GradeHistory { get; set; }
    public ObservableCollection<System.Windows.Controls.TabItem> TabItems { get; set; }

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

    public StudentDashboardViewModel(ITestService testService)
    {
        _testService = testService;
        _availableRoles = new ObservableCollection<string> { "Student" };

        UpcomingTestItems = new ObservableCollection<TestItem>();
        CompletedTestItems = new ObservableCollection<TestItem>();

        GradeHistory = new ObservableCollection<GradeDataPoint>
        {
            new GradeDataPoint { TestName = "History Essay", Score = 85, TestDate = _baseDate.AddDays(20) },
            new GradeDataPoint { TestName = "Physics Lab", Score = 78, TestDate = _baseDate.AddDays(25) },
            new GradeDataPoint { TestName = "Literature Quiz", Score = 88, TestDate = _baseDate.AddDays(28) },
            new GradeDataPoint { TestName = "Statistics Midterm", Score = 92, TestDate = _baseDate.AddDays(30) }
        };

        TabItems = new ObservableCollection<System.Windows.Controls.TabItem>
        {
            new System.Windows.Controls.TabItem { Header = "Available" },
            new System.Windows.Controls.TabItem { Header = "Completed" },
            new System.Windows.Controls.TabItem { Header = "Leaderboard" },
            new System.Windows.Controls.TabItem { Header = "Statistics" }
        };

        _ = LoadUpcomingTestsAsync();
        _ = LoadCompletedTestsAsync();
    }

    private async Task LoadUpcomingTestsAsync()
    {
        try
        {
            IsLoading = true;
            
            var upcomingTests = await _testService.GetUpcomingTestsAsync();
            
            UpcomingTestItems.Clear();
            foreach (var test in upcomingTests)
            {
                UpcomingTestItems.Add(new TestItem
                {
                    TestId = test.TestId,
                    Icon = "??",
                    TestName = test.Name,
                    SubjectName = test.Subject,
                    DateInfo = test.NumberOfQuestions > 0 ? $"{test.NumberOfQuestions} questions" : "No questions",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                });
            }

            UpcomingTests = UpcomingTestItems.Count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading upcoming tests: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCompletedTestsAsync()
    {
        try
        {
            var completedTests = await _testService.GetCompletedTestsAsync();
            
            CompletedTestItems.Clear();
            foreach (var test in completedTests)
            {
                CompletedTestItems.Add(new TestItem
                {
                    TestId = test.TestId,
                    Icon = "??",
                    TestName = test.TestName,
                    SubjectName = test.SubjectName,
                    DateInfo = $"Completed: {test.CompletedDate:MMM d, yyyy}",
                    StatusOrScore = $"{test.ScorePercentage:F1}%",
                    StatusLabel = "GRADE"
                });
            }

            CompletedTests = CompletedTestItems.Count;
            
            // Calculate average score from completed tests
            if (CompletedTests > 0)
            {
                AverageScore = CompletedTestItems.Average(t => 
                {
                    var scoreStr = t.StatusOrScore.TrimEnd('%');
                    return double.TryParse(scoreStr, out var score) ? score : 0;
                });
                
                // Calculate grade from average
                CurrentGrade = AverageScore >= 90 ? "A" :
                              AverageScore >= 80 ? "B" :
                              AverageScore >= 70 ? "C" :
                              AverageScore >= 60 ? "D" : "F";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading completed tests: {ex.Message}");
        }
    }

    public async Task RefreshTestsAsync()
    {
        await LoadUpcomingTestsAsync();
        await LoadCompletedTestsAsync();
    }

    public void ShowTest(object testView)
    {
        CurrentView = testView;
        IsShowingTest = true;
    }

    public void ReturnToDashboard()
    {
        CurrentView = null;
        IsShowingTest = false;
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
