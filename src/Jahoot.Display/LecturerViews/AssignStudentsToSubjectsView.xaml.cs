using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Jahoot.Display.LecturerViews
{
    public partial class AssignStudentsToSubjectsView : UserControl
    {
        public AssignStudentsToSubjectsView()
        {
            InitializeComponent();
            if (App.Current is App app)
            {
                DataContext = app.ServiceProvider.GetService<AssignStudentsToSubjectsViewModel>();
            }
        }
    }
}
