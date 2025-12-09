using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using Jahoot.Core.Models.Requests;

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
        LoginErrorBanner.Visibility = Visibility.Collapsed;

        if (!new EmailAddressAttribute().IsValid(SignInEmailTextBox.Text))
        {
            LoginErrorText.Text = "Please enter a valid email address.";
            LoginErrorBanner.Visibility = Visibility.Visible;
            return;
        }

        var loginRequest = new LoginRequestModel
        {
            Email = SignInEmailTextBox.Text,
            Password = SignInPasswordBox.Password
        };

        try
        {
            var result = await _authService.Login(loginRequest);

            if (result.Success)
            {
                _lecturerDashboard.Show();
                this.Close();
            }
            else
            {
                LoginErrorText.Text = result.ErrorMessage;
                LoginErrorBanner.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            LoginErrorText.Text = $"An error occurred: {ex.Message}";
            LoginErrorBanner.Visibility = Visibility.Visible;
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
