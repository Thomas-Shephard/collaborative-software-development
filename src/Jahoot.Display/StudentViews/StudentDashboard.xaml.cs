using System.Windows;
using System.Windows.Controls;
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
            // Tab selection is handled by data binding in the ViewModel
            // This method is kept for potential future UI-specific logic
        }
    }
}
