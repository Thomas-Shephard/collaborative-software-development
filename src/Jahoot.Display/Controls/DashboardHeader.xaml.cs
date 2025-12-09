using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

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
                if (currentWindow == null || !currentWindow.IsLoaded) return;

                var app = Application.Current as App;
                if (app?.ServiceProvider == null) return;
                
                // Determine which dashboard to navigate to
                Window? newDashboard = newRole switch
                {
                    "Student" when currentWindow is not StudentViews.StudentDashboard 
                        => app.ServiceProvider.GetRequiredService<StudentViews.StudentDashboard>(),
                    "Lecturer" when currentWindow is not LecturerViews.LecturerDashboard 
                        => app.ServiceProvider.GetRequiredService<LecturerViews.LecturerDashboard>(),
                    _ => null
                };

                if (newDashboard != null)
                {
                    newDashboard.Show();
                    currentWindow.Close();
                }
            }
            catch
            {
                // Silently fail if role switching doesn't work
                // This prevents crashes during initialization
            }
        }

        public DashboardHeader()
        {
            InitializeComponent();
        }
    }
}
