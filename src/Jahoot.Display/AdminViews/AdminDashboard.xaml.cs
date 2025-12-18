using System.Windows;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.Pages
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard(ISubjectService subjectService, ILecturerService lecturerService)
        {
            InitializeComponent();
            this.DataContext = new AdminDashboardViewModel();
            _ = InitializeViewsAsync(subjectService, lecturerService);
        }

        private async Task InitializeViewsAsync(ISubjectService subjectService, ILecturerService lecturerService)
        {
            await SubjectsView.Initialize(subjectService);
            await LecturersView.Initialize(lecturerService);
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AdminDashboardViewModel vm)
            {
                // Ensure UI elements are initialized
                if (SubjectsView == null || LecturersView == null)
                    return;

                int index = vm.SelectedTabIndex;
                
                UpdateVisibility(index);
            }
        }
        
        private void UpdateVisibility(int index)
        {
             SubjectsView.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
             LecturersView.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}