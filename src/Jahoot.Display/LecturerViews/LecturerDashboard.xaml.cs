using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        public LecturerDashboard()
        {
            InitializeComponent();
            this.DataContext = new LecturerDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Future functionality
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
        public string LecturerInitials
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "JD";

        public int TotalStudents
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 120;

        public int ActiveTests
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 5;

        public double AverageScore
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 78.5;

        public string CompletionRate
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "85%";


        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }

        public LecturerDashboardViewModel()
        {
            RecentActivityItems = new ObservableCollection<RecentActivityItem>
            {
                new RecentActivityItem { StudentInitials = "AS", DescriptionPrefix = "Student ", TestName = "Math Quiz", TimeAgo = "5 mins ago", Result = "100%" },
                new RecentActivityItem { StudentInitials = "BM", DescriptionPrefix = "Student ", TestName = "Science Test", TimeAgo = "1 hour ago", Result = "85%" },
                new RecentActivityItem { StudentInitials = "CJ", DescriptionPrefix = "Student ", TestName = "History Exam", TimeAgo = "2 hours ago", Result = "72%" }
            };

            PerformanceSubjects = new ObservableCollection<PerformanceSubject>
            {
                new PerformanceSubject { SubjectName = "Mathematics", ScoreText = "88%", ScoreValue = 88 },
                new PerformanceSubject { SubjectName = "Science", ScoreText = "75%", ScoreValue = 75 },
                new PerformanceSubject { SubjectName = "History", ScoreText = "60%", ScoreValue = 60 },
                new PerformanceSubject { SubjectName = "English", ScoreText = "92%", ScoreValue = 92 }
            };
        }
    }

    public class RecentActivityItem
    {
        public required string StudentInitials { get; set; }
        public required string DescriptionPrefix { get; set; }
        public required string TestName { get; set; }
        public required string TimeAgo { get; set; }
        public required string Result { get; set; }
    }

    public class PerformanceSubject
    {
        public required string SubjectName { get; set; }
        public required string ScoreText { get; set; }
        public required double ScoreValue { get; set; }
    }
}
