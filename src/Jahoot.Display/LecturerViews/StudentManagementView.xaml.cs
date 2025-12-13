using Jahoot.Core.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Jahoot.Display.LecturerViews
{
    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            StudentsListView.ItemsSource = new List<Student>
            {
                new Student { UserId = 1, Name = "John Smith", Email = "john.smith@example.com", StudentId = 1001, LastLogin = new System.DateTime(2025, 12, 1, 10, 30, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 2, Name = "Jane Doe", Email = "jane.doe@example.com", StudentId = 1002, LastLogin = new System.DateTime(2025, 12, 2, 14, 0, 0), AccountStatus = StudentAccountStatus.Active, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
                new Student { UserId = 3, Name = "Peter Jones", Email = "peter.jones@example.com", StudentId = 1003, LastLogin = new System.DateTime(2025, 11, 30, 9, 15, 0), AccountStatus = StudentAccountStatus.Disabled, CreatedAt = new System.DateTime(2025, 1, 1), UpdatedAt = new System.DateTime(2025, 1, 1), PasswordHash = "", Roles = new List<Role>() },
            };
        }
    }
}
