using System.Windows;

namespace Jahoot.Display.LecturerViews
{

    public partial class CreateTestWindow : Window
    {
        public CreateTestWindow(CreateTestViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += CreateTestWindow_Loaded;
        }

        private async void CreateTestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateTestViewModel viewModel)
            {
                await viewModel.InitialiseAsync();
            }
        }
    }
}
