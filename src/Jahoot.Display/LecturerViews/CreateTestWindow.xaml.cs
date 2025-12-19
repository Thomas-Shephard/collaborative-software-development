using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    /// <summary>
    /// Interaction logic for CreateTestWindow.xaml
    /// </summary>
    public partial class CreateTestWindow : Window
    {
        public CreateTestWindow(CreateTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
