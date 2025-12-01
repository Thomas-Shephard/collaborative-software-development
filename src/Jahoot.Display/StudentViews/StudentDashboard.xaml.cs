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
            DataContext = new StudentDashboardViewModel();
        }

        {
        }
    }
}
