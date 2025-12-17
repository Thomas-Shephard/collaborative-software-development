using Jahoot.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jahoot.Display.LecturerViews
{

    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            DataContext = new StudentManagementViewModel();
        }
    }

    public class StudentManagementViewModel : BaseViewModel
    {
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();
        public ObservableCollection<Student> Students
        {
            get { return _students; }
            set
            {
                _students = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditStudentCommand { get; }
        public ICommand ApproveStudentCommand { get; }
        public ICommand RejectStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand EnableStudentCommand { get; }
        public ICommand DisableStudentCommand { get; }


        public StudentManagementViewModel()
        {
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
                var studentInCollection = Students.FirstOrDefault(s => s.UserId == studentToReject.UserId);
                if (studentInCollection != null)
                {
                    Students.Remove(studentInCollection);
                }
            }
        }

        private void DeleteStudent(object? obj)
        {
            if (obj is Student studentToDelete)
            {
                var studentInCollection = Students.FirstOrDefault(s => s.UserId == studentToDelete.UserId);
                if (studentInCollection != null)
                {
                    Students.Remove(studentInCollection);
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
}
