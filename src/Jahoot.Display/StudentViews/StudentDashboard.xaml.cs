using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class StudentDashboard : Window
    {
        public StudentDashboard()
        {
            InitializeComponent();
            MainTabs.SelectedIndex = 0;
            MainTabs_SelectionChanged(MainTabs, new SelectionChangedEventArgs(System.Windows.Controls.TabControl.SelectionChangedEvent, new List<object>(), new List<object>()));
            DataContext = new StudentDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (MainTabs.SelectedItem is not TabItem selectedTab)
            {
                return;
            }

            if (selectedTab.Header is not string header)
            {
                return;
            }

            switch (header)
            {
                case "Available Tests":
                    ShowAvailableTests();
                    break;
                case "Completed Tests":
                    ShowCompletedTests();
                    break;
                case "Leaderboard":
                    ShowLeaderboard();
                    break;
                case "Statistics":
                    ShowStatistics();
                    break;
            }
        }

        private void ShowAvailableTests()
        {
            var itemsControl = new ItemsControl
            {
                ItemsSource = _viewModel.UpcomingTests,
                ItemTemplate = (DataTemplate)FindResource("AvailableTestTemplate")
            };
            if (MainContent == null)
            {
                return;
            }
            MainContent.Content = itemsControl;
        }

        private void ShowCompletedTests()
        {
            var itemsControl = new ItemsControl
            {
                ItemsSource = _viewModel.CompletedTests,
                ItemTemplate = (DataTemplate)FindResource("CompletedTestTemplate")
            };
            if (MainContent == null)
            {
                return;
            }
            MainContent.Content = itemsControl;
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Two-way binding handles the tab selection logic in the ViewModel.
            // This handler is kept for any future UI-specific logic if needed.
        }
    }
}
