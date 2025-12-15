using Jahoot.Core.Models;
using Jahoot.Display.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Data;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        public LecturerDashboard()
        {
            InitializeComponent();
            this.DataContext = new LecturerDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LecturerDashboardViewModel viewModel &&
                sender is NavigationalTabs tabs &&
                tabs.MainTabsControl.SelectedItem is NavigationTabItem selectedTab)
            {
                Type viewType = selectedTab.ViewType;
                if (viewType == null) return;

                UserControl? view = Activator.CreateInstance(viewType) as UserControl;
                if (view == null) return;

                view.DataContext = viewModel;
                viewModel.CurrentView = view;
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
        private object _currentView = null!;
        public string LecturerInitials { get; set; } = "JD";
        public int TotalStudents { get; set; } = 120;
        public int ActiveTests { get; set; } = 5;
        public double AverageScore { get; set; } = 78.5;
        public double CompletionRate { get; set; } = 85;

        public ObservableCollection<RecentActivityItem> RecentActivityItems { get; set; }
        public ObservableCollection<PerformanceSubject> PerformanceSubjects { get; set; }
        public ObservableCollection<NavigationTabItem> TabItems { get; set; }
        public ObservableCollection<Student> Students { get; set; }

        public ICommand EditStudentCommand { get; }
        public ICommand ApproveStudentCommand { get; }
        public ICommand RejectStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand EnableStudentCommand { get; }
        public ICommand DisableStudentCommand { get; }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AvailableRoles { get; set; } = new ObservableCollection<string> { "Student", "Lecturer", "Admin" };
        public string SelectedRole { get; set; } = "Lecturer";
        public int SelectedTabIndex { get; set; } = 0;

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

            TabItems = new ObservableCollection<NavigationTabItem>
            {
                new NavigationTabItem { Header = "Overview", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Students", ViewType = typeof(StudentManagementView) },
                new NavigationTabItem { Header = "Tests", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Progress", ViewType = typeof(LecturerOverviewView) },
                new NavigationTabItem { Header = "Leaderboard", ViewType = typeof(LecturerOverviewView) }
            };

            Students = new ObservableCollection<Student>
            {
                new Student { UserId = 1, Name = "John Smith", Email = "john.smith@example.com", StudentId = 1001, LastLogin = new System.DateTime(2025, 12, 1, 10, 30, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 2, Name = "Jane Doe", Email = "jane.doe@example.com", StudentId = 1002, LastLogin = new System.DateTime(2025, 12, 2, 14, 0, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 3, Name = "Peter Jones", Email = "peter.jones@example.com", StudentId = 1003, LastLogin = new System.DateTime(2025, 11, 30, 9, 15, 0), AccountStatus = StudentAccountStatus.Disabled, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 4, Name = "Alice Williams", Email = "alice.williams@example.com", StudentId = 1004, LastLogin = new System.DateTime(2025, 12, 3, 11, 0, 0), AccountStatus = StudentAccountStatus.PendingApproval, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 5, Name = "Bob Brown", Email = "bob.brown@example.com", StudentId = 1005, LastLogin = new System.DateTime(2025, 12, 3, 12, 45, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 6, Name = "Charlie Davis", Email = "charlie.davis@example.com", StudentId = 1006, LastLogin = new System.DateTime(2025, 12, 3, 13, 20, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 7, Name = "Diana Miller", Email = "diana.miller@example.com", StudentId = 1007, LastLogin = new System.DateTime(2025, 12, 2, 8, 10, 0), AccountStatus = StudentAccountStatus.PendingApproval, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 8, Name = "Ethan Wilson", Email = "ethan.wilson@example.com", StudentId = 1008, LastLogin = new System.DateTime(2025, 11, 29, 18, 5, 0), AccountStatus = StudentAccountStatus.Disabled, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 9, Name = "Fiona Taylor", Email = "fiona.taylor@example.com", StudentId = 1009, LastLogin = new System.DateTime(2025, 12, 1, 16, 30, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 10, Name = "George Anderson", Email = "george.anderson@example.com", StudentId = 1010, LastLogin = new System.DateTime(2025, 12, 3, 14, 55, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 11, Name = "Hannah Thomas", Email = "hannah.thomas@example.com", StudentId = 1011, LastLogin = new System.DateTime(2025, 12, 2, 11, 25, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 12, Name = "Ian Jackson", Email = "ian.jackson@example.com", StudentId = 1012, LastLogin = new System.DateTime(2025, 12, 3, 10, 10, 0), AccountStatus = StudentAccountStatus.PendingApproval, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 13, Name = "Jessica White", Email = "jessica.white@example.com", StudentId = 1013, LastLogin = new System.DateTime(2025, 11, 28, 13, 40, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 14, Name = "Kevin Harris", Email = "kevin.harris@example.com", StudentId = 1014, LastLogin = new System.DateTime(2025, 12, 3, 9, 5, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 15, Name = "Laura Martin", Email = "laura.martin@example.com", StudentId = 1015, LastLogin = new System.DateTime(2025, 12, 1, 19, 20, 0), AccountStatus = StudentAccountStatus.Disabled, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 16, Name = "Megan Thompson", Email = "megan.thompson@example.com", StudentId = 1016, LastLogin = new System.DateTime(2025, 12, 3, 15, 50, 0), AccountStatus = StudentAccountStatus.PendingApproval, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 17, Name = "Nathan Garcia", Email = "nathan.garcia@example.com", StudentId = 1017, LastLogin = new System.DateTime(2025, 12, 2, 17, 15, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 18, Name = "Olivia Martinez", Email = "olivia.martinez@example.com", StudentId = 1018, LastLogin = new System.DateTime(2025, 12, 3, 16, 25, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 19, Name = "Paul Rodriguez", Email = "paul.rodriguez@example.com", StudentId = 1019, LastLogin = new System.DateTime(2025, 11, 27, 12, 0, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 20, Name = "Quincy Lee", Email = "quincy.lee@example.com", StudentId = 1020, LastLogin = new System.DateTime(2025, 12, 3, 17, 30, 0), AccountStatus = StudentAccountStatus.PendingApproval, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() }
            };

            EditStudentCommand = new RelayCommand(EditStudent);
            ApproveStudentCommand = new RelayCommand(ApproveStudent);
            RejectStudentCommand = new RelayCommand(RejectStudent);
            DeleteStudentCommand = new RelayCommand(DeleteStudent);
            EnableStudentCommand = new RelayCommand(EnableStudent);
            DisableStudentCommand = new RelayCommand(DisableStudent);

            CurrentView = new LecturerOverviewView { DataContext = this };
        }

        private void EditStudent(object? obj)
        {
            if (obj is Student student)
            {
                var editWindow = new EditStudentWindow(student);
                editWindow.ShowDialog();
            }
        }

        private void ApproveStudent(object? obj)
        {
            if (obj is Student student)
            {
                student.AccountStatus = StudentAccountStatus.Active;
            }
        }

        private void RejectStudent(object? obj)
        {
            if (obj is Student studentToReject)
            {
                Debug.WriteLine($"Attempting to reject student: {studentToReject.Name} (ID: {studentToReject.UserId})");
                var studentInCollection = Students.FirstOrDefault(s => s.UserId == studentToReject.UserId);
                if (studentInCollection != null)
                {
                    bool removed = Students.Remove(studentInCollection);
                    Debug.WriteLine($"Student removed: {removed}. Current student count: {Students.Count}");
                    if (removed)
                    {
                        // Explicitly refresh the CollectionView to force UI update
                        ICollectionView view = CollectionViewSource.GetDefaultView(Students);
                        view.Refresh();
                    }
                }
                else
                {
                    Debug.WriteLine($"Student with ID {studentToReject.UserId} not found in collection.");
                }
            }
        }

        private void DeleteStudent(object? obj)
        {
            if (obj is Student studentToDelete)
            {
                Debug.WriteLine($"Attempting to delete student: {studentToDelete.Name} (ID: {studentToDelete.UserId})");
                var studentInCollection = Students.FirstOrDefault(s => s.UserId == studentToDelete.UserId);
                if (studentInCollection != null)
                {
                    bool removed = Students.Remove(studentInCollection);
                    Debug.WriteLine($"Student removed: {removed}. Current student count: {Students.Count}");
                    if (removed)
                    {
                        // Explicitly refresh the CollectionView to force UI update
                        ICollectionView view = CollectionViewSource.GetDefaultView(Students);
                        view.Refresh();
                    }
                }
                else
                {
                    Debug.WriteLine($"Student with ID {studentToDelete.UserId} not found in collection.");
                }
            }
        }

        private void EnableStudent(object? obj)
        {
            if (obj is Student student)
            {
                student.AccountStatus = StudentAccountStatus.Active;
            }
        }

        private void DisableStudent(object? obj)
        {
            if (obj is Student student)
            {
                student.AccountStatus = StudentAccountStatus.Disabled;
            }
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

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}