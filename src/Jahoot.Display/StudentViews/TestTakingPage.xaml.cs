using System.Windows;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class TestTakingPage : Window
    {
        public TestTakingPage()
        {
            InitializeComponent();
            DataContext = new TestTakingViewModel();
        }

        public TestTakingPage(TestTakingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
