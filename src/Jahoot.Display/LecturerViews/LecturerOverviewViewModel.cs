using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Jahoot.Display.Models; // For RecentActivityItem and PerformanceSubject
using Jahoot.WebApi.Models.Responses; // For CompletedTestResponse and LeaderboardEntry

namespace Jahoot.Display.LecturerViews
{
    public class LecturerOverviewViewModel : BaseViewModel
    {
        private readonly ISubjectService _subjectService;
        private readonly ITestService _testService;

        private ObservableCollection<RecentActivityItem> _recentActivityItems = new();
        public ObservableCollection<RecentActivityItem> RecentActivityItems
        {
            get => _recentActivityItems;
            set
            {
                _recentActivityItems = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PerformanceSubject> _performanceSubjects = new();
        public ObservableCollection<PerformanceSubject> PerformanceSubjects
        {
            get => _performanceSubjects;
            set
            {
                _performanceSubjects = value;
                OnPropertyChanged();
            }
        }

        public LecturerOverviewViewModel(ISubjectService subjectService, ITestService testService)
        {
            _subjectService = subjectService;
            _testService = testService;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                // Fetch Recent Activity
                var completedTests = await _testService.GetRecentCompletedTests();
                RecentActivityItems = new ObservableCollection<RecentActivityItem>(
                    completedTests.Select(ct => new RecentActivityItem
                    {
                        StudentInitials = GetInitials(ct.StudentName),
                        DescriptionPrefix = "Student ",
                        TestName = ct.TestName,
                        TimeAgo = FormatTimeAgo(ct.CompletedDate),
                        Result = $"{ct.ScorePercentage:F0}%"
                    })
                );

                // Fetch Performance Overview
                var subjects = await _subjectService.GetSubjects();
                var performanceData = new List<PerformanceSubject>();
                foreach (var subject in subjects)
                {
                    var leaderboard = await _subjectService.GetLeaderboardForSubject(subject.SubjectId);
                    if (leaderboard.Any())
                    {
                        double averageScore = leaderboard.Average(entry => entry.TotalScore);
                        performanceData.Add(new PerformanceSubject
                        {
                            SubjectName = subject.Name,
                            ScoreText = $"{averageScore:F0}%", // Format to whole number percentage
                            ScoreValue = averageScore
                        });
                    }
                    else
                    {
                        performanceData.Add(new PerformanceSubject
                        {
                            SubjectName = subject.Name,
                            ScoreText = "N/A",
                            ScoreValue = 0
                        });
                    }
                }
                PerformanceSubjects = new ObservableCollection<PerformanceSubject>(performanceData);
            }
            catch (Exception ex)
            {
                // In a real application, this would be logged or shown in a more user-friendly way
                System.Windows.MessageBox.Show($"Error loading overview data: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return string.Empty;
        }

        private string FormatTimeAgo(DateTime completionTime)
        {
            TimeSpan timeSinceCompletion = DateTime.UtcNow - completionTime.ToUniversalTime();

            if (timeSinceCompletion.TotalMinutes < 1) return "just now";
            if (timeSinceCompletion.TotalMinutes < 60) return $"{timeSinceCompletion.Minutes} mins ago";
            if (timeSinceCompletion.TotalHours < 24) return $"{timeSinceCompletion.Hours} hours ago";
            if (timeSinceCompletion.TotalDays < 7) return $"{timeSinceCompletion.Days} days ago";
            return completionTime.ToLocalTime().ToString("MMM d, yyyy");
        }
    }
}
