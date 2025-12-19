using System.Windows;

namespace Jahoot.Display.LecturerViews
{

    public partial class EditTestWindow : Window
    {
        public EditTestWindow(EditTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += EditTestWindow_Loaded;
        }

        private async void EditTestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditTestViewModel viewModel)
            {
                await viewModel.InitialiseAsync();
            }
        }
    }
}
