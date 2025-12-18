using System.Collections.ObjectModel;
using System.Windows.Controls;
using Jahoot.Display.Models;

namespace Jahoot.Display.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private ObservableCollection<string> _availableRoles = new();
        
        public string AdminInitials
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "AD";

        public int TotalUsers
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 350;

        public int TotalSubjects
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 42;

        public string SystemHealth
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "99.9%";

        public int ActiveSessions
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 12;

        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

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
        } = "Admin";

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public AdminDashboardViewModel()
        {
            // Initialize with Admin role by default
            _availableRoles = new ObservableCollection<string> { "Admin" };
            
            RecentActivityItems = new ObservableCollection<RecentActivityItem>
            {
                new RecentActivityItem { StudentInitials = "Sy", DescriptionPrefix = "System ", TestName = "Backup Completed", TimeAgo = "10 mins ago", Result = "Success" },
                new RecentActivityItem { StudentInitials = "Us", DescriptionPrefix = "User ", TestName = "New Registration", TimeAgo = "1 hour ago", Result = "Pending" },
                 new RecentActivityItem { StudentInitials = "Al", DescriptionPrefix = "Alert ", TestName = "High CPU Usage", TimeAgo = "2 hours ago", Result = "Resolved" }
            };

            PerformanceSubjects = new ObservableCollection<PerformanceSubject>
            {
                new PerformanceSubject { SubjectName = "Server Load", ScoreText = "Low", ScoreValue = 20 },
                new PerformanceSubject { SubjectName = "Memory Usage", ScoreText = "45%", ScoreValue = 45 },
                 new PerformanceSubject { SubjectName = "Disk Space", ScoreText = "60%", ScoreValue = 60 }
            };

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Overview" },
                new TabItem { Header = "Manage Subjects" },
                new TabItem { Header = "Manage Lecturers" },
                new TabItem { Header = "Settings" }
            };
        }
    }
}
