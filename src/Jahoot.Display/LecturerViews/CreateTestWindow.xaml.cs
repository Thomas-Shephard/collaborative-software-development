using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

