using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Jahoot.Display.Commands;
using Jahoot.Display.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        private TestTakingViewModel? _currentTestViewModel;

        public StudentDashboard(ITestService testService, IUserRoleService? userRoleService = null)
        {
            InitializeComponent();
            
            var viewModel = new StudentDashboardViewModel(testService);
            
            if (userRoleService != null)
            {
                var availableDashboards = userRoleService.GetAvailableDashboards();
                viewModel.AvailableRoles = new ObservableCollection<string>(availableDashboards);
            }
            
            viewModel.TestItemClickCommand = new RelayCommand(OnTestItemClick, CanClickTestItem);
            
            DataContext = viewModel;
            
            // Cleanup when window closes
            Closed += (s, e) => CleanupTestViewModel();
        }

        private void CleanupTestViewModel()
        {
            if (_currentTestViewModel != null)
            {
                _currentTestViewModel.ShowResults -= OnShowResults;
                _currentTestViewModel.TestSubmitted -= OnTestSubmitted;
                _currentTestViewModel = null;
            }
        }

        private bool CanClickTestItem(object? parameter)
        {
            return parameter is TestItem;
        }

        private async void OnTestItemClick(object? parameter)
        {
            if (parameter is not TestItem testItem)
            {
                return;
            }

            if (testItem.StatusLabel == "NOT TAKEN")
            {
                try
                {
                    // Cleanup previous test view model if exists
                    CleanupTestViewModel();

                    var app = Application.Current as App;
                    var testService = app?.ServiceProvider?.GetRequiredService<ITestService>();
                    
                    if (testService == null)
                    {
                        Debug.WriteLine("Unable to load test service.");
                        return;
                    }

                    _currentTestViewModel = new TestTakingViewModel(testService);
                    
                    // Subscribe to events with named methods for easier cleanup
                    _currentTestViewModel.ShowResults += OnShowResults;
                    _currentTestViewModel.TestSubmitted += OnTestSubmitted;
                    
                    // Load the test
                    await _currentTestViewModel.LoadTestAsync(testItem.TestId, testItem.TestName, testItem.SubjectName);
                    
                    // Create test view and show it inline
                    var testView = new TestTakingView
                    {
                        DataContext = _currentTestViewModel
                    };
                    
                    if (DataContext is StudentDashboardViewModel dashboardViewModel)
                    {
                        dashboardViewModel.ShowTest(testView);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening test: {ex.Message}");
                    CleanupTestViewModel();
                }
            }
            else
            {
                // Show completed test results summary
                // Note: Can't show question-by-question review because student answers aren't stored in database
                var dashboardVM = DataContext as StudentDashboardViewModel;
                var completedTestData = dashboardVM?.CompletedTestItems.FirstOrDefault(t => t.TestId == testItem.TestId);
                
                if (completedTestData != null)
                {
                    // Parse the score percentage
                    var scoreStr = completedTestData.StatusOrScore.TrimEnd('%');
                    if (!double.TryParse(scoreStr, out var scorePercentage))
                    {
                        scorePercentage = 0;
                    }
                    
                    // Parse the completion date from DateInfo (format: "Completed: MMM d, yyyy")
                    var dateStr = completedTestData.DateInfo.Replace("Completed: ", "");
                    if (!DateTime.TryParse(dateStr, out var completedDate))
                    {
                        completedDate = DateTime.Now;
                    }
                    
                    // Create a summary with available information
                    // Note: TotalQuestions, CorrectAnswers, and IncorrectAnswers are not available
                    // for completed tests since detailed results aren't stored in the database
                    var summary = new TestResultSummary
                    {
                        TestName = completedTestData.TestName,
                        SubjectName = completedTestData.SubjectName,
                        TotalQuestions = 0, // Not available for completed tests
                        CorrectAnswers = 0, // Not available
                        IncorrectAnswers = 0, // Not available
                        ScorePercentage = scorePercentage,
                        Grade = scorePercentage >= 90 ? "A" :
                               scorePercentage >= 80 ? "B" :
                               scorePercentage >= 70 ? "C" :
                               scorePercentage >= 60 ? "D" : "F",
                        CompletedAt = completedDate,
                        TimeTaken = TimeSpan.Zero // Not stored
                    };
                    
                    // Show results with empty review list (student answers not stored in database)
                    ShowResultsInline(summary, new List<QuestionReviewItem>());
                }
                else
                {
                    Debug.WriteLine("Unable to load test results - test data not found.");
                }
            }
        }

        private void ShowResultsInline(TestResultSummary resultSummary, List<QuestionReviewItem> reviewItems)
        {
            var resultsView = new TestResultsView(resultSummary, reviewItems);
            
            if (DataContext is not StudentDashboardViewModel dashboardViewModel)
                return;

            // Subscribe to return to dashboard
            resultsView.ReturnToDashboardRequested += (sender, args) =>
            {
                dashboardViewModel.ReturnToDashboard();
            };
            
            dashboardViewModel.ShowTest(resultsView);
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is StudentDashboardViewModel viewModel)
            {
                viewModel.UpdateTabVisibility(viewModel.SelectedTabIndex);
            }
        }

        // Named event handler methods for easier subscription management
        private void OnShowResults(object? sender, (TestResultSummary Summary, List<QuestionReviewItem> ReviewItems) data)
        {
            if (DataContext is StudentDashboardViewModel dashboardViewModel)
            {
                ShowResultsInline(data.Summary, data.ReviewItems);
            }
        }

        private async void OnTestSubmitted(object? sender, EventArgs args)
        {
            if (DataContext is StudentDashboardViewModel dashboardViewModel)
            {
                await dashboardViewModel.RefreshTestsAsync();
            }
            
            // Cleanup after test completion
            CleanupTestViewModel();
        }
    }
}
