using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    public partial class TestManagementView : UserControl
    {
        public TestManagementView()
        {
            InitializeComponent();
            DataContext = ((App)App.Current).ServiceProvider.GetRequiredService<TestManagementViewModel>();
            Loaded += TestManagementView_Loaded;
        }

        private async void TestManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TestManagementViewModel viewModel)
            {
                await viewModel.InitialiseAsync();
            }
        }
    }
}
