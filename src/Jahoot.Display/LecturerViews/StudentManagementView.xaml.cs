using System.Windows.Controls;

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
}