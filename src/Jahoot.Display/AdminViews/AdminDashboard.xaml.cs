using System.Windows;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Jahoot.Display.Pages
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard(ISubjectService subjectService, ILecturerService lecturerService)
        {
            InitializeComponent();
            
            // Get user roles from service and filter available roles
            var app = Application.Current as App;
            var userRoleService = app?.ServiceProvider?.GetService<IUserRoleService>();
            
            var viewModel = new AdminDashboardViewModel();
            
            if (userRoleService != null)
            {
                var availableDashboards = userRoleService.GetAvailableDashboards();
                viewModel.AvailableRoles = new ObservableCollection<string>(availableDashboards);
            }
            
            DataContext = viewModel;
            _ = InitializeViewsAsync(subjectService, lecturerService);
        }

        private async Task InitializeViewsAsync(ISubjectService subjectService, ILecturerService lecturerService)
        {
            await SubjectsView.Initialize(subjectService);
            await LecturersView.Initialize(lecturerService);
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminDashboardViewModel vm)
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
