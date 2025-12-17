using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        public LecturerDashboard()
        {
            InitializeComponent();
            this.DataContext = new LecturerDashboardViewModel();
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Future functionality
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Press F5 to open test taking page for testing
            if (e.Key == System.Windows.Input.Key.F5)
            {
                var testPage = new StudentViews.TestTakingPage();
                testPage.Show();
            }
        }
    }
}
