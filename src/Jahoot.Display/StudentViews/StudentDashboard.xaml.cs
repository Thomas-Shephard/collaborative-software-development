using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Jahoot.Display.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;
using Jahoot.Display.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
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
                    var app = Application.Current as App;
                    var testService = app?.ServiceProvider?.GetRequiredService<ITestService>();
                    
                    if (testService == null)
                    {
                        Debug.WriteLine("Unable to load test service.");
                        return;
                    }

                    var testViewModel = new TestTakingViewModel(testService);
                    
                    // Subscribe to show results inline
                    testViewModel.ShowResults += (sender, data) =>
                    {
                        if (DataContext is StudentDashboardViewModel dashboardViewModel)
                        {
                            ShowResultsInline(data.Summary, data.ReviewItems);
                        }
                    };
                    
                    // Subscribe to test submission to refresh and return to dashboard
                    testViewModel.TestSubmitted += async (sender, args) =>
                    {
                        // Refresh the test list after submission
                        if (DataContext is StudentDashboardViewModel dashboardViewModel)
                        {
                            await dashboardViewModel.RefreshTestsAsync();
                        }
                    };
                    
                    // Load the test
                    await testViewModel.LoadTestAsync(testItem.TestId, testItem.TestName, testItem.SubjectName);
                    
                    // Create test view and show it inline
                    var testView = new TestTakingView
                    {
                        DataContext = testViewModel
                    };
                    
                    if (DataContext is StudentDashboardViewModel dashboardViewModel2)
                    {
                        dashboardViewModel2.ShowTest(testView);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening test: {ex.Message}");
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
                    DateTime completedDate = DateTime.Now;
                    var dateStr = completedTestData.DateInfo.Replace("Completed: ", "");
                    DateTime.TryParse(dateStr, out completedDate);
                    
                    // Create a summary with available information
                    var summary = new TestResultSummary
                    {
                        TestName = completedTestData.TestName,
                        SubjectName = completedTestData.SubjectName,
                        TotalQuestions = 1, // Unknown - using 1 as placeholder
                        CorrectAnswers = scorePercentage >= 50 ? 1 : 0, // Approximation
                        IncorrectAnswers = scorePercentage < 50 ? 1 : 0, // Approximation
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
            
            // Subscribe to return to dashboard
            resultsView.ReturnToDashboardRequested += (sender, args) =>
            {
                if (DataContext is StudentDashboardViewModel dashboardViewModel)
                {
                    dashboardViewModel.ReturnToDashboard();
                }
            };
            
            if (DataContext is StudentDashboardViewModel dashboardViewModel2)
            {
                dashboardViewModel2.ShowTest(resultsView);
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
}
