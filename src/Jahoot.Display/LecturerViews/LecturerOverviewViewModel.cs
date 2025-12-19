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
                IEnumerable<Jahoot.WebApi.Models.Responses.CompletedTestResponse> completedTests = new List<Jahoot.WebApi.Models.Responses.CompletedTestResponse>();
                try
                {
                    completedTests = await _testService.GetRecentCompletedTests();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error fetching recent activity: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                
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
                IEnumerable<Subject> subjects = new List<Subject>();
                try
                {
                    subjects = await _subjectService.GetSubjects();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error fetching subjects for performance overview: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }

                var performanceData = new List<PerformanceSubject>();
                foreach (var subject in subjects)
                {
                    IEnumerable<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
                    try
                    {
                        leaderboard = await _subjectService.GetLeaderboardForSubject(subject.SubjectId);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error fetching leaderboard for subject {subject.Name}: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }

                    if (leaderboard.Any())
                    {
                        double averageScore = leaderboard.Average(entry => entry.TotalScore);
                        performanceData.Add(new PerformanceSubject
                        {
                            SubjectName = subject.Name,
                            ScoreText = $"{averageScore:F0}%",
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
                System.Windows.MessageBox.Show($"An unknown error occurred during overview data initialization: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;

            string initials = string.Empty;
            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    initials += char.ToUpper(part[0]);
                    if (initials.Length >= 2)
                    {
                        break;
                    }
                }
            }

            return initials;
        }

        private static string FormatTimeAgo(DateTime completionTime)
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
