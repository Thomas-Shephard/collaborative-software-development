using Jahoot.Core.Models;
using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;

namespace Jahoot.Display;
public partial class LoginPage : Window
{
    private readonly IAuthService _authService;

    public LoginPage(IAuthService authService)
    {
        InitializeComponent(); 
        _authService = authService;
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

        var loginRequest = new LoginRequest
        {
            Email = SignInEmailTextBox.Text, 
            Password = SignInPasswordBox.Password 
        };

        try
        {
            var result = await _authService.Login(loginRequest);

            if (result.Success)
            {
                // Get the app and create the dashboard when needed
                var app = (App)Application.Current;
                var lecturerDashboard = app.ServiceProvider.GetRequiredService<LecturerViews.LecturerDashboard>();
                lecturerDashboard.Show();
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
