using System.Windows.Controls;
using System.Windows;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerOverviewView : UserControl
    {
        private readonly LecturerOverviewViewModel _viewModel;

        public LecturerOverviewView(LecturerOverviewViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Loaded += LecturerOverviewView_Loaded;
            // The DataContext is set by the parent (LecturerDashboardViewModel)
            // But we need to ensure the ViewModel is initialized.
            // DataContext = viewModel; // This would override what parent sets if set here
        }

        private async void LecturerOverviewView_Loaded(object sender, RoutedEventArgs e)
        {
            // It's crucial that the DataContext is correctly set by the parent
            // before this Loaded event fires, so the ViewModel is available.
            if (DataContext is LecturerOverviewViewModel viewModel)
            {
                await viewModel.InitialiseAsync();
            }
        }
    }
}
