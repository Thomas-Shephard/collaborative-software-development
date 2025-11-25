using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System;
using System.Windows;

namespace Jahoot.Display;
/// <summary>
/// This is the main window of our app. It's where users can log in.
/// </summary>
public partial class LoginPage : Window
{
    private readonly IAuthService _authService;
    private readonly LecturerViews.LecturerDashboard _lecturerDashboard;

    /// <summary>
    /// Sets up the main window. It gets the login service ready.
    /// </summary>
    /// <param name="authService">The login service we need.</param>
    public LoginPage(IAuthService authService, LecturerViews.LecturerDashboard lecturerDashboard)
    {
        InitializeComponent(); // Get all the buttons and text boxes ready.
        _authService = authService; // Keep hold of the login service.
        _lecturerDashboard = lecturerDashboard;
    }
    /// <summary>
    /// This happens when the "Login" button is clicked.
    /// It tries to log the user in.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">Extra info about the click.</param>
    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // Grab the email and password the user typed in.
        var loginRequest = new LoginRequest
        {
            Email = SignInEmailTextBox.Text, // Get the email from the text box.
            Password = SignInPasswordBox.Password // Get the password from the password box.
        };

        try
        {
            // Ask the login service to try and log us in.
            var (success, message) = await _authService.Login(loginRequest);

            // Tell the user if it worked or not.
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
}
