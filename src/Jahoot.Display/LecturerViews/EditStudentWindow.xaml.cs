using Jahoot.Core.Models;
using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    public partial class EditStudentWindow : Window
    {
        private Student _originalStudent;
        public Student StudentCopy { get; set; }

        public EditStudentWindow(Student student)
        {
            InitializeComponent();
            _originalStudent = student;
            // Create a copy to edit
            StudentCopy = new Student
            {
                UserId = student.UserId,
                Name = student.Name,
                Email = student.Email,
                StudentId = student.StudentId,
                LastLogin = student.LastLogin,
                AccountStatus = student.AccountStatus,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt,
                PasswordHash = student.PasswordHash,
                Roles = student.Roles // Assuming Roles is a reference type that doesn't need deep copy for this context
            };
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Copy changes back to the original student
            _originalStudent.Name = StudentCopy.Name;
            _originalStudent.Email = StudentCopy.Email;
            // Note: AccountStatus is no longer editable in the popup, but keeping it for completeness

            DialogResult = true;
            Close();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            // Do nothing, original student remains unchanged
            DialogResult = false;
            Close();
        }
    }
}
