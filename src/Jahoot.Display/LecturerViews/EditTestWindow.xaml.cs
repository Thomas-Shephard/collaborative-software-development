using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    /// <summary>
    /// Interaction logic for EditTestWindow.xaml
    /// </summary>
    public partial class EditTestWindow : Window
    {
        public EditTestWindow(EditTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
