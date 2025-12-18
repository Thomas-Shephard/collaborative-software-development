using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    public partial class DashboardHeader : UserControl
    {
        public static readonly DependencyProperty AvailableRolesProperty =
            DependencyProperty.Register(nameof(AvailableRoles), typeof(ObservableCollection<string>), typeof(DashboardHeader), new FrameworkPropertyMetadata(new ObservableCollection<string>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ObservableCollection<string> AvailableRoles
        {
            get => (ObservableCollection<string>)GetValue(AvailableRolesProperty);
            set => SetValue(AvailableRolesProperty, value);
        }

        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register(nameof(SelectedRole), typeof(string), typeof(DashboardHeader), new FrameworkPropertyMetadata("Lecturer", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedRoleChanged));

        public string SelectedRole
        {
            get => (string)GetValue(SelectedRoleProperty);
            set => SetValue(SelectedRoleProperty, value);
        }

        public static readonly DependencyProperty UserInitialsProperty =
            DependencyProperty.Register(nameof(UserInitials), typeof(string), typeof(DashboardHeader), new PropertyMetadata("JD"));

        public string UserInitials
        {
            get => (string)GetValue(UserInitialsProperty);
            set => SetValue(UserInitialsProperty, value);
        }

        public static readonly DependencyProperty SubHeaderTextProperty =
            DependencyProperty.Register(nameof(SubHeaderText), typeof(string), typeof(DashboardHeader), new FrameworkPropertyMetadata("Manage students, tests, and monitor progress", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SubHeaderText
        {
            get => (string)GetValue(SubHeaderTextProperty);
            set => SetValue(SubHeaderTextProperty, value);
        }

        private static void OnSelectedRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DashboardHeader header && e.NewValue is string newRole && e.OldValue is string oldRole && newRole != oldRole)
            {
                header.HandleRoleChange(newRole);
            }
        }

        private void HandleRoleChange(string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
            {
                Debug.WriteLine("[DashboardHeader] Role change aborted: new role is null or empty");
                return;
            }

            try
            {
                var currentWindow = Window.GetWindow(this);
                if (currentWindow == null || !currentWindow.IsLoaded)
                {
                    Debug.WriteLine($"[DashboardHeader] Cannot change role: Window is null or not loaded. Role: {newRole}");
                    return;
                }

                var app = Application.Current as App;
                if (app?.ServiceProvider == null)
                {
                    Debug.WriteLine("[DashboardHeader] Cannot change role: ServiceProvider is null");
                    return;
                }

                // Check if user has access to the requested dashboard
                var userRoleService = app.ServiceProvider.GetService<IUserRoleService>();
                if (userRoleService != null && !userRoleService.HasAccessToDashboard(newRole))
                {
                    Debug.WriteLine($"[DashboardHeader] Access denied: User does not have {newRole} role");
                    MessageBox.Show($"You do not have access to the {newRole} dashboard.", 
                        "Access Denied", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    
                    // Revert to previous role
                    SelectedRole = (string)GetValue(SelectedRoleProperty);
                    return;
                }

                var navigationService = app.ServiceProvider.GetRequiredService<IDashboardNavigationService>();
                
                bool success = navigationService.NavigateToDashboard(newRole, currentWindow);
                
                if (!success)
                {
                    Debug.WriteLine($"[DashboardHeader] Navigation to {newRole} dashboard failed");
                    MessageBox.Show($"Failed to navigate to {newRole} dashboard.", 
                        "Navigation Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[DashboardHeader] Service resolution failed: {ex.Message}");
                Trace.TraceError($"DashboardHeader role change failed - DI issue: {ex}");
                MessageBox.Show("An error occurred while changing dashboards.", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"[DashboardHeader] Invalid navigation parameter: {ex.Message}");
                Trace.TraceError($"DashboardHeader role change failed - Invalid argument: {ex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardHeader] Unexpected error during role change: {ex.Message}");
                Trace.TraceError($"DashboardHeader role change failed - Unexpected error: {ex}");
                MessageBox.Show("An unexpected error occurred.", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        public DashboardHeader()
        {
            InitializeComponent();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var app = (App)Application.Current;
                var authService = app.ServiceProvider.GetRequiredService<IAuthService>();
                var userRoleService = app.ServiceProvider.GetService<IUserRoleService>();

                await authService.Logout();
                
                // Clear user roles on logout
                userRoleService?.ClearRoles();

                MessageBox.Show("You have been successfully signed out", "Signed Out", MessageBoxButton.OK, MessageBoxImage.Information);

                var landingPage = app.ServiceProvider.GetRequiredService<LandingPage>();
                landingPage.Show();

                var currentWindow = Window.GetWindow(this);
                currentWindow?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardHeader] Logout failed: {ex.Message}");
                MessageBox.Show("Logout failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
