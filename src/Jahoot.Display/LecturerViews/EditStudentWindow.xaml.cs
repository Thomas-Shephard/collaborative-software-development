using Jahoot.Core.Models;
using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    public partial class EditStudentWindow : Window
    {
        public Student Student { get; set; }

        public EditStudentWindow(Student student)
        {
            InitializeComponent();
            Student = student;
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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
