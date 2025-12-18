using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class TestTakingPage : Window
    {
        public TestTakingPage() : this(new TestTakingViewModel())
        {
        }

        public TestTakingPage(TestTakingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            viewModel.TestSubmitted += OnTestSubmitted;
            
            Closed += (s, e) => viewModel.TestSubmitted -= OnTestSubmitted;
        }

        private void OnTestSubmitted(object? sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
