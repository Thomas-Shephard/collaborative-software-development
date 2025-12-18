using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Jahoot.Display.LecturerViews
{
    public partial class TestManagementView : UserControl
    {
        public TestManagementView()
        {
            InitializeComponent();
            DataContext = ((App)App.Current).ServiceProvider.GetRequiredService<TestManagementViewModel>();
        }
    }
}
