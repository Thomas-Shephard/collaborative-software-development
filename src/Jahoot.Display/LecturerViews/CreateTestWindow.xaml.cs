using System.Windows;

namespace Jahoot.Display.LecturerViews
{

    public partial class CreateTestWindow : Window
    {
        public CreateTestWindow(CreateTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
