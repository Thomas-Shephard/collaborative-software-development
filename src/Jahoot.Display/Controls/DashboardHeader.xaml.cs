using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
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

        private static void OnSelectedRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DashboardHeader header) return;

            var newRole = e.NewValue as string;
            if (string.IsNullOrEmpty(newRole)) return;

            var currentWindow = Window.GetWindow(header);
            if (currentWindow == null) return;

            Type? targetWindowType = null;
            if (newRole == "Lecturer")
                targetWindowType = typeof(LecturerViews.LecturerDashboard);
            else if (newRole == "Admin")
                targetWindowType = typeof(Pages.AdminDashboard);

            if (targetWindowType != null && currentWindow.GetType() != targetWindowType)
            {
                var app = (App)Application.Current;
                var newWindow = app.ServiceProvider.GetRequiredService(targetWindowType) as Window;
                
                if (newWindow != null)
                {
                    newWindow.Show();
                    currentWindow.Close();
                }
            }
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

                await authService.Logout();

                MessageBox.Show("You have been successfully signed out", "Signed Out", MessageBoxButton.OK, MessageBoxImage.Information);

                var loginPage = app.ServiceProvider.GetRequiredService<LoginPage>();
                loginPage.Show();

                var currentWindow = Window.GetWindow(this);
                currentWindow?.Close();
            }
            catch
            {
                MessageBox.Show($"Logout failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
