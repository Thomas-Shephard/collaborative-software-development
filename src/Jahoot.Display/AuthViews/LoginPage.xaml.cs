using Jahoot.Display.Services;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display;
public partial class LoginPage : Window
{
    private readonly IAuthService _authService;
    private readonly LecturerViews.LecturerDashboard _lecturerDashboard;
    private readonly IServiceProvider _serviceProvider;

    public LoginPage(IAuthService authService, LecturerViews.LecturerDashboard lecturerDashboard, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authService = authService;
        _lecturerDashboard = lecturerDashboard;
        _serviceProvider = serviceProvider;
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
            var result = await _authService.Login(loginRequest, RememberMeCheckBox.IsChecked ?? false);

            if (result.Success)
            {
                _lecturerDashboard.Show();
                Close();
            }
            else
            {
                LoginErrorText.Text = result.ErrorMessage;
                LoginErrorBanner.Visibility = Visibility.Visible;
            }
        }
        catch 
        {
            LoginErrorText.Text = $"An error occurred.";
            LoginErrorBanner.Visibility = Visibility.Visible;
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        LoginErrorBanner.Visibility = Visibility.Collapsed;

        var name = RegisterFullNameTextBox.Text;
        var email = RegisterEmailTextBox.Text;
        var password = RegisterPasswordBox.Password;
        var confirmPassword = RegisterConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(name) || name.Length > 70)
        {
            LoginErrorText.Text = name.Length > 70 ? "Full Name cannot exceed 70 characters." : "Full Name is required.";
            LoginErrorBanner.Visibility = Visibility.Visible;
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
            LoginErrorText.Text = "Please enter a valid email address.";
            LoginErrorBanner.Visibility = Visibility.Visible;
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            LoginErrorText.Text = "Password is required.";
            LoginErrorBanner.Visibility = Visibility.Visible;
            return;
        }

        var strongPasswordAttribute = new StrongPasswordAttribute();
        if (!strongPasswordAttribute.IsValid(password))
        {
            LoginErrorText.Text = strongPasswordAttribute.ErrorMessage;
            LoginErrorBanner.Visibility = Visibility.Visible;
            return;
        }
        if (password != confirmPassword)
        {
            LoginErrorText.Text = "Passwords do not match.";
            LoginErrorBanner.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var result = await _authService.Register(new CreateStudentRequestModel
            {
                Name = name,
                Email = email,
                Password = password
            });

            if (result.Success)
            {
                MessageBox.Show("Registration successful! Please sign in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RegisterFullNameTextBox.Clear();
                RegisterEmailTextBox.Clear();
                RegisterPasswordBox.Clear();
                RegisterConfirmPasswordBox.Clear();
            }
            else
            {
                LoginErrorText.Text = result.ErrorMessage;
                LoginErrorBanner.Visibility = Visibility.Visible;
            }
        }
        catch
        {
            LoginErrorText.Text = $"An error occurred.";
            LoginErrorBanner.Visibility = Visibility.Visible;
        }
    }

    private void ForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        var forgotPasswordWindow = ActivatorUtilities.CreateInstance<ForgotPasswordInitialWindow>(_serviceProvider);
        forgotPasswordWindow.Owner = this;
        forgotPasswordWindow.ShowDialog();
    }

    private void SignInPasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            LoginButton_Click(sender, e);
        }
    }
}
