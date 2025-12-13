using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        private readonly Services.ISubjectService _subjectService;

        public LecturerDashboard(Services.ISubjectService subjectService)
        {
            InitializeComponent();
            _subjectService = subjectService;
            var viewModel = new LecturerDashboardViewModel(subjectService);
            this.DataContext = viewModel;
            SubjectsView.Initialize(subjectService);

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LecturerDashboardViewModel.SelectedRole))
            {
                if (DataContext is LecturerDashboardViewModel viewModel)
                {
                    UpdateViewForRole(viewModel.SelectedRole);
                }
            }
        }

        private void UpdateViewForRole(string role)
        {
            if (DataContext is LecturerDashboardViewModel viewModel)
            {
                if (role == "Subjects")
                {
                    viewModel.HeaderDescription = "View and manage subjects";
                    StatsGrid.Visibility = Visibility.Collapsed;
                    MainTabs.Visibility = Visibility.Collapsed;
                    OverviewContent.Visibility = Visibility.Collapsed;
                    SubjectsView.Visibility = Visibility.Visible;
                }
                else
                {
                    viewModel.HeaderDescription = "Manage students, tests, and monitor progress";
                    StatsGrid.Visibility = Visibility.Visible;
                    MainTabs.Visibility = Visibility.Visible;
                    SubjectsView.Visibility = Visibility.Collapsed;
                    
                    // Restore logic for tabs
                    MainTabs_SelectionChanged(MainTabs, new RoutedEventArgs());
                }
            }
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LecturerDashboardViewModel viewModel)
            {
                viewModel.SelectedTabIndex = MainTabs.SelectedIndex;

                if (viewModel.TabItems.Count > MainTabs.SelectedIndex && MainTabs.SelectedIndex >= 0)
                {
                    var selectedTabHeader = viewModel.TabItems[MainTabs.SelectedIndex].Header.ToString();

                    if (selectedTabHeader == "Subjects")
                    {
                        OverviewContent.Visibility = Visibility.Collapsed;
                        SubjectsView.Visibility = Visibility.Visible;
                    }
                    else if (selectedTabHeader == "Overview")
                    {
                        OverviewContent.Visibility = Visibility.Visible;
                        SubjectsView.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        OverviewContent.Visibility = Visibility.Collapsed;
                        SubjectsView.Visibility = Visibility.Collapsed;
                    }
                }
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

    public class LecturerDashboardViewModel : BaseViewModel
    {
        private readonly Services.ISubjectService _subjectService;

        public string HeaderDescription
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Manage students, tests, and monitor progress";

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

        public double CompletionRate
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 85;


        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

        public ObservableCollection<string> AvailableRoles
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = new ObservableCollection<string> { "Student", "Lecturer", "Admin", "Subjects" };

        public string SelectedRole
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Lecturer";

        public int SelectedTabIndex
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 0;

        public LecturerDashboardViewModel(Services.ISubjectService subjectService)
        {
            _subjectService = subjectService;

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
            
            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Overview" },
                new TabItem { Header = "Students" },
                new TabItem { Header = "Tests" },
                new TabItem { Header = "Progress" },
                new TabItem { Header = "Leaderboard" }
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
