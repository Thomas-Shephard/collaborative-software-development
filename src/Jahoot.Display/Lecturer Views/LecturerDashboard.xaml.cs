using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Lecturer_Views
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

    public class LecturerDashboardViewModel : INotifyPropertyChanged
    {
        private string _lecturerInitials = "JD";
        public string LecturerInitials
        {
            get => _lecturerInitials;
            set
            {
                _lecturerInitials = value;
                OnPropertyChanged(nameof(LecturerInitials));
            }
        }

        private int _totalStudents = 120;
        public int TotalStudents
        {
            get => _totalStudents;
            set
            {
                _totalStudents = value;
                OnPropertyChanged(nameof(TotalStudents));
            }
        }

        private int _activeTests = 5;
        public int ActiveTests
        {
            get => _activeTests;
            set
            {
                _activeTests = value;
                OnPropertyChanged(nameof(ActiveTests));
            }
        }

        private double _averageScore = 78.5;
        public double AverageScore
        {
            get => _averageScore;
            set
            {
                _averageScore = value;
                OnPropertyChanged(nameof(AverageScore));
            }
        }

        private string _completionRate = "85%";
        public string CompletionRate
        {
            get => _completionRate;
            set
            {
                _completionRate = value;
                OnPropertyChanged(nameof(CompletionRate));
            }
        }

        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }

        public LecturerDashboardViewModel()
        {
            RecentActivityItems = new ObservableCollection<RecentActivityItem>
            {
                new RecentActivityItem { StudentInitials = "AS", DescriptionPrefix = "Student ", TestName = "Math Quiz", TimeAgo = "5 mins ago", IsInProgress = true, ResultOrStatus = "In Progress" },
                new RecentActivityItem { StudentInitials = "BM", DescriptionPrefix = "Student ", TestName = "Science Test", TimeAgo = "1 hour ago", IsInProgress = false, ResultOrStatus = "85%" },
                new RecentActivityItem { StudentInitials = "CJ", DescriptionPrefix = "Student ", TestName = "History Exam", TimeAgo = "2 hours ago", IsInProgress = false, ResultOrStatus = "72%" }
            };

            PerformanceSubjects = new ObservableCollection<PerformanceSubject>
            {
                new PerformanceSubject { SubjectName = "Mathematics", ScoreText = "88%", ScoreValue = 88 },
                new PerformanceSubject { SubjectName = "Science", ScoreText = "75%", ScoreValue = 75 },
                new PerformanceSubject { SubjectName = "History", ScoreText = "60%", ScoreValue = 60 },
                new PerformanceSubject { SubjectName = "English", ScoreText = "92%", ScoreValue = 92 }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RecentActivityItem
    {
        public string StudentInitials { get; set; }
        public string DescriptionPrefix { get; set; }
        public string TestName { get; set; }
        public string TimeAgo { get; set; }
        public bool IsInProgress { get; set; }
        public string ResultOrStatus { get; set; }
    }

    public class PerformanceSubject
    {
        public string SubjectName { get; set; }
        public string ScoreText { get; set; }
        public double ScoreValue { get; set; }
    }
}
