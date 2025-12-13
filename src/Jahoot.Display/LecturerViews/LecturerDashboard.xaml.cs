using Jahoot.Display.ViewModels;
using Jahoot.Display.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.LecturerViews
{
    public partial class LecturerDashboard : Window
    {
        private const string RoleSubjects = "Subjects";
        private const string TabOverview = "Overview";

        public LecturerDashboard(ISubjectService subjectService)
        {
            InitializeComponent();
            var viewModel = new LecturerDashboardViewModel(subjectService);
            this.DataContext = viewModel;
            SubjectsView.Initialize(subjectService);

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LecturerDashboardViewModel.SelectedRole) && DataContext is LecturerDashboardViewModel viewModel)
            {
                UpdateViewForRole(viewModel.SelectedRole);
            }
        }

        private void UpdateViewForRole(string role)
        {
            if (DataContext is LecturerDashboardViewModel viewModel)
            {
                if (role == RoleSubjects)
                {
                    viewModel.HeaderDescription = "View and manage subjects";
                    UpdateVisibility(true);
                }
                else
                {
                    viewModel.HeaderDescription = "Manage students, tests, and monitor progress";
                    UpdateVisibility(false);
                    
                    // Restore logic for tabs
                    MainTabs_SelectionChanged(MainTabs, new RoutedEventArgs());
                }
            }
        }

        private void UpdateVisibility(bool showSubjects)
        {
            if (showSubjects)
            {
                StatsGrid.Visibility = Visibility.Collapsed;
                MainTabs.Visibility = Visibility.Collapsed;
                OverviewContent.Visibility = Visibility.Collapsed;
                SubjectsView.Visibility = Visibility.Visible;
            }
            else
            {
                StatsGrid.Visibility = Visibility.Visible;
                MainTabs.Visibility = Visibility.Visible;
                SubjectsView.Visibility = Visibility.Collapsed;
            }
        }

        private void MainTabs_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LecturerDashboardViewModel viewModel)
            {
                viewModel.SelectedTabIndex = MainTabs.SelectedIndex;

                if (viewModel.TabItems.Count > MainTabs.SelectedIndex && MainTabs.SelectedIndex >= 0)
                {
                    var selectedTabHeader = viewModel.TabItems[MainTabs.SelectedIndex].Header.ToString();

                    if (selectedTabHeader == TabOverview)
                    {
                        OverviewContent.Visibility = Visibility.Visible;
                        SubjectsView.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        OverviewContent.Visibility = Visibility.Collapsed;
                        SubjectsView.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}