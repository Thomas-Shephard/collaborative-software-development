using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        private readonly StudentDashboardViewModel _viewModel;

        public StudentDashboard()
        {
            _viewModel = new StudentDashboardViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            MainTabs.SelectedIndex = 0;
            MainTabs_SelectionChanged(MainTabs, new SelectionChangedEventArgs(System.Windows.Controls.TabControl.SelectionChangedEvent, new List<object>(), new List<object>()));
            DataContext = new StudentDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Two-way binding handles the tab selection logic in the ViewModel.
            // This handler is kept for any future UI-specific logic if needed.
        }
    }
}
