using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Jahoot.Display.Models;

namespace Jahoot.Display.ViewModels
{
    public class StudentDashboardViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TestItem> UpcomingTests { get; } = new();
        public ObservableCollection<TestItem> CompletedTests { get; } = new();

        public StudentDashboardViewModel()
        {
            // Sample data for demo / tests
            UpcomingTests.Add(new TestItem{ Title = "Math Midterm", Date = DateTime.Today.AddDays(3), Course = "Math 101" });
            UpcomingTests.Add(new TestItem{ Title = "History Quiz", Date = DateTime.Today.AddDays(7), Course = "History 201" });

            CompletedTests.Add(new TestItem{ Title = "Intro Quiz", Date = DateTime.Today.AddDays(-14), Score = 78, Course = "Math 101" });
            CompletedTests.Add(new TestItem{ Title = "Chapter 1 Test", Date = DateTime.Today.AddDays(-10), Score = 85, Course = "History 201" });
            CompletedTests.Add(new TestItem{ Title = "Pop Quiz", Date = DateTime.Today.AddDays(-3), Score = 92, Course = "Math 101" });

        }

        public double AverageScore
        {
            get
            {
                var scores = CompletedTests.Select(t => t.Score).Where(s => s.HasValue).Select(s => s!.Value).ToList();
                if (!scores.Any()) return 0;
                return Math.Round(scores.Average(), 1);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
