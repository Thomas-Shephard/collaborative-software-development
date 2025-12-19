using System.Windows;

namespace Jahoot.Display.LecturerViews
{

    public partial class EditTestWindow : Window
    {
        public EditTestWindow(EditTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
