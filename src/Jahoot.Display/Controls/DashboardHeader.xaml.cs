using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Jahoot.Core.Models;
using Jahoot.Display.Services;

namespace Jahoot.Display.Controls
{
    public partial class DashboardHeader : UserControl
    {
        public static readonly DependencyProperty AvailableRolesProperty =
            DependencyProperty.Register(nameof(AvailableRoles), typeof(ObservableCollection<string>), typeof(DashboardHeader), new FrameworkPropertyMetadata(new ObservableCollection<string>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ObservableCollection<string> AvailableRoles
        {
            get { return (ObservableCollection<string>)GetValue(AvailableRolesProperty); }
            set { SetValue(AvailableRolesProperty, value); }
        }

        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register(nameof(SelectedRole), typeof(string), typeof(DashboardHeader), new FrameworkPropertyMetadata("Lecturer", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedRoleChanged));

        public string SelectedRole
        {
            get { return (string)GetValue(SelectedRoleProperty); }
            set { SetValue(SelectedRoleProperty, value); }
        }

        public static readonly DependencyProperty UserInitialsProperty =
            DependencyProperty.Register(nameof(UserInitials), typeof(string), typeof(DashboardHeader), new PropertyMetadata("JD"));

        public string UserInitials
        {
            get { return (string)GetValue(UserInitialsProperty); }
            set { SetValue(UserInitialsProperty, value); }
        }

        public static readonly DependencyProperty SubHeaderTextProperty =
            DependencyProperty.Register(nameof(SubHeaderText), typeof(string), typeof(DashboardHeader), new FrameworkPropertyMetadata("Manage students, tests, and monitor progress", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SubHeaderText
        {
            get { return (string)GetValue(SubHeaderTextProperty); }
            set { SetValue(SubHeaderTextProperty, value); }
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

                var navigationService = app.ServiceProvider.GetRequiredService<IDashboardNavigationService>();
                
                bool success = navigationService.NavigateToDashboard(newRole, currentWindow);
                
                if (!success)
                {
                    Debug.WriteLine($"[DashboardHeader] Navigation to {newRole} dashboard failed");
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[DashboardHeader] Service resolution failed: {ex.Message}");
                Trace.TraceError($"DashboardHeader role change failed - DI issue: {ex}");
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
            }
        }

        public DashboardHeader()
        {
            InitializeComponent();
        }
    }
}
