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
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void ShowLeaderboard()
        {
            if (MainContent == null)
            {
                return;
            }
            MainContent.Content = new TextBlock { Text = "Leaderboard coming soon...", Margin = new Thickness(20) };
        }



        private void ShowStatistics()
        {
            if (MainContent == null)
            {
                return;
            }
            MainContent.Content = new TextBlock { Text = "Statistics coming soon...", Margin = new Thickness(20) };
        }
    }
}
