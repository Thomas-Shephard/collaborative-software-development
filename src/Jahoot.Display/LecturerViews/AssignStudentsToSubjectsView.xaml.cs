using Jahoot.Display.ViewModels;
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

        private async void Assign_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AssignStudentsToSubjectsViewModel viewModel)
            {
                var selectedItems = StudentListBox.SelectedItems.Cast<Core.Models.Student>().ToList();
                await viewModel.AssignStudents(selectedItems);
                
                if (viewModel.IsFeedbackSuccess)
                {
                    StudentListBox.UnselectAll();
                }
            }
        }
    }
}
