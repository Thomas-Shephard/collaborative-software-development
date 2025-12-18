using Jahoot.Display.Services;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Jahoot.Display;

public partial class ForgotPasswordInitialWindow : Window
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public ForgotPasswordInitialWindow(IAuthService authService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
    {
        string email = EmailTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
            ShowError("Please enter a valid email address.");
            return;
        }

        SendCodeButton.IsEnabled = false;
        SendCodeButton.Content = "Sending...";
        HideMessages();

        try
        {
            var result = await _authService.ForgotPassword(email);
            if (result.Success)
            {
                var resetWindow = ActivatorUtilities.CreateInstance<ForgotPasswordFinaliseWindow>(_serviceProvider);
                resetWindow.PreFillEmail(email);

                // Close this window and show the next one
                resetWindow.Show();
                Close();
            }
            else
            {
                ShowError(result.ErrorMessage ?? "An error occurred.");
            }
        }
        catch (Exception)
        {
             ShowError("An unexpected error occurred. Please try again later.");
        }
        finally
        {
            SendCodeButton.IsEnabled = true;
            SendCodeButton.Content = "Send Code";
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
        SuccessBanner.Visibility = Visibility.Collapsed;
    }
    
    private void HideMessages()
    {
        ErrorBanner.Visibility = Visibility.Collapsed;
        SuccessBanner.Visibility = Visibility.Collapsed;
    }
}
