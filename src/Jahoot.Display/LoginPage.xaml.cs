using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System.Windows;
using System.Windows.Input;

namespace Jahoot.Display;
public partial class LoginPage : Window
{
    private readonly IAuthService _authService;
    private readonly LecturerViews.LecturerDashboard _lecturerDashboard;

    public LoginPage(IAuthService authService, LecturerViews.LecturerDashboard lecturerDashboard)
    {
        InitializeComponent(); 
        _authService = authService; 
        _lecturerDashboard = lecturerDashboard;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var loginRequest = new LoginRequest
        {
            Email = SignInEmailTextBox.Text, 
            Password = SignInPasswordBox.Password 
        };

        try
        {
            var (success, message) = await _authService.Login(loginRequest);

            if (success)
            {
                _lecturerDashboard.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show($"Login failed: {message}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Register button clicked!");
    }

    private void ForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("forgot password clicked");
    }

    private void SignInPasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            LoginButton_Click(sender, e);
        }
    }
}
