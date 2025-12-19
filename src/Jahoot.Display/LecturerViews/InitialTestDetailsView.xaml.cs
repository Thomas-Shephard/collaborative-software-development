using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display.LecturerViews
{
    public partial class InitialTestDetailsView : UserControl
    {
        public InitialTestDetailsView()
        {
            InitializeComponent();
            Loaded += InitialTestDetailsView_Loaded;
        }

        private async void InitialTestDetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InitialTestDetailsViewModel viewModel)
            {
                await viewModel.InitialiseAsync();
            }
        }
    }
}
