using Jahoot.Display.Services;
using System;
using System.Windows;
using Jahoot.Core.Attributes;

namespace Jahoot.Display;

public partial class ForgotPasswordFinaliseWindow : Window
{
    private readonly IAuthService _authService;

    public ForgotPasswordFinaliseWindow(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    public void PreFillEmail(string email)
    {
        EmailTextBox.Text = email;
    }

    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        string email = EmailTextBox.Text.Trim();
        string token = TokenTextBox.Text.Trim();
        string password = NewPasswordBox.Password;
        string confirmPassword = ConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(token))
        {
            ShowError("Please enter the reset code.");
            return;
        }

        var strongPasswordAttribute = new StrongPasswordAttribute();
        if (!strongPasswordAttribute.IsValid(password))
        {
            ShowError(strongPasswordAttribute.ErrorMessage ?? "Password does not meet complexity requirements.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Passwords do not match.");
            return;
        }

        ResetButton.IsEnabled = false;
        ResetButton.Content = "Reseting...";
        HideMessages();

        try
        {
            var result = await _authService.ResetPassword(email, token, password);
            if (result.Success)
            {
                MessageBox.Show("Password reset successfully! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Failed to reset password.");
            }
        }
        catch (Exception)
        {
            ShowError("An unexpected error occurred.");
        }
        finally
        {
            ResetButton.IsEnabled = true;
            ResetButton.Content = "Reset Password";
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBanner.Visibility = Visibility.Visible;
    }

    private void HideMessages()
    {
        ErrorBanner.Visibility = Visibility.Collapsed;
        SuccessBanner.Visibility = Visibility.Collapsed;
    }
}
