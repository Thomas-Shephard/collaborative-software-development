using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        public StudentDashboard()
        {
            InitializeComponent();
            DataContext = new StudentDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Two-way binding handles the tab selection logic in the ViewModel.
            // This handler is kept for any future UI-specific logic if needed.
        }
    }
}
