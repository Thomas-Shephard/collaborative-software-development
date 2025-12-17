using Jahoot.Core.Models;
using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    public partial class EditStudentWindow : Window
    {
        private readonly Student _originalStudent;
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
                IsApproved = student.IsApproved,
                IsDisabled = student.IsDisabled,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt,
                PasswordHash = student.PasswordHash,
                Roles = student.Roles,
                Subjects = student.Subjects
            };
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _originalStudent.Name = StudentCopy.Name;
            _originalStudent.Email = StudentCopy.Email;
            DialogResult = true;
            Close();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
