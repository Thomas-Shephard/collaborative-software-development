using Jahoot.Display.Models;
using Jahoot.Display.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Jahoot.Display.ViewModels;

public class LecturerDashboardViewModel : BaseViewModel
{
    private readonly ISubjectService _subjectService;

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

    public LecturerDashboardViewModel(ISubjectService subjectService)
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
