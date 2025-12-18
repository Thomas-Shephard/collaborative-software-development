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
                if (OverviewContent == null || SubjectsView == null || LecturersView == null || SettingsView == null)
                    return;

                int index = vm.SelectedTabIndex;
                
                UpdateVisibility(index);
            }
        }
        
        private void UpdateVisibility(int index)
        {
             OverviewContent.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
             SubjectsView.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
             LecturersView.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
             SettingsView.Visibility = index == 3 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
