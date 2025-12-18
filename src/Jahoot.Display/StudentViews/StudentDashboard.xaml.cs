using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Models;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        public StudentDashboard()
        {
            InitializeComponent();
            this.DataContext = new StudentDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
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

    public class StudentDashboardViewModel : BaseViewModel
    {
        public string StudentInitials
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "ST";

        public int UpcomingTests
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 3;

        public int CompletedTests
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 12;

        public double AverageScore
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 82.5;

        public string CurrentGrade
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "B+";

        public ObservableCollection<TestItem> UpcomingTestItems { get; set; }
        public ObservableCollection<TestItem> CompletedTestItems { get; set; }
        public ObservableCollection<TabItem> TabItems { get; set; }

        public ObservableCollection<string> AvailableRoles
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = new ObservableCollection<string> { "Student", "Lecturer", "Admin" };

        public string SelectedRole
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Student";

        public int SelectedTabIndex
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = 0;

        public StudentDashboardViewModel()
        {
            UpcomingTestItems = new ObservableCollection<TestItem>
            {
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Mathematics Final Exam", 
                    SubjectName = "Advanced Calculus", 
                    DateInfo = "Due: Dec 15, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Chemistry Quiz", 
                    SubjectName = "Organic Chemistry", 
                    DateInfo = "Due: Dec 18, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Programming Assignment", 
                    SubjectName = "Data Structures", 
                    DateInfo = "Due: Dec 20, 2024",
                    StatusOrScore = "Pending",
                    StatusLabel = "NOT TAKEN"
                }
            };

            CompletedTestItems = new ObservableCollection<TestItem>
            {
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Statistics Midterm", 
                    SubjectName = "Applied Statistics", 
                    DateInfo = "Completed: Dec 1, 2024",
                    StatusOrScore = "92%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "History Essay", 
                    SubjectName = "World History", 
                    DateInfo = "Completed: Nov 28, 2024",
                    StatusOrScore = "85%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "Physics Lab Report", 
                    SubjectName = "General Physics", 
                    DateInfo = "Completed: Nov 25, 2024",
                    StatusOrScore = "78%",
                    StatusLabel = "GRADE"
                },
                new TestItem 
                { 
                    Icon = "??", 
                    TestName = "English Literature Quiz", 
                    SubjectName = "Modern Literature", 
                    DateInfo = "Completed: Nov 20, 2024",
                    StatusOrScore = "88%",
                    StatusLabel = "GRADE"
                }
            };

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Overview" },
                new TabItem { Header = "My Tests" },
                new TabItem { Header = "Performance" },
                new TabItem { Header = "Calendar" }
            };
        }
    }
}
