using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class TestTakingPage : Window
    {
        public TestTakingPage()
        {
            InitializeComponent();
            var viewModel = new TestTakingViewModel();
            DataContext = viewModel;
            
            // Subscribe to TestSubmitted event to close window and return to dashboard
            viewModel.TestSubmitted += OnTestSubmitted;
            
            // Clean up event subscription when window closes
            Closed += (s, e) => viewModel.TestSubmitted -= OnTestSubmitted;
        }

        public TestTakingPage(TestTakingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Subscribe to TestSubmitted event to close window and return to dashboard
            viewModel.TestSubmitted += OnTestSubmitted;
            
            // Clean up event subscription when window closes
            Closed += (s, e) => viewModel.TestSubmitted -= OnTestSubmitted;
        }

        private void OnTestSubmitted(object? sender, System.EventArgs e)
        {
            // Close the test page and return to dashboard
            this.Close();
        }
    }
}
