using Jahoot.Display.Services;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Jahoot.Display;
public partial class LandingPage : Window
{
    private readonly IAuthService _authService;
    private readonly LecturerViews.LecturerDashboard _lecturerDashboard;
    private readonly IServiceProvider _serviceProvider;

    public LandingPage(IAuthService authService, LecturerViews.LecturerDashboard lecturerDashboard, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authService = authService;
        _lecturerDashboard = lecturerDashboard;
        _serviceProvider = serviceProvider;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        FeedbackBox.Visibility = Visibility.Collapsed;
        FeedbackBox.IsSuccess = false;

        if (!new EmailAddressAttribute().IsValid(SignInEmailTextBox.Text))
        {
            FeedbackBox.Message = "Please enter a valid email address.";
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
                Close();
            }
            else
            {
                if (result.ErrorMessage?.Contains("approved") == true)
                {
                     FeedbackBox.Message = "Your account hasn't been approved yet.";
                }
                else
                {
                     FeedbackBox.Message = result.ErrorMessage ?? "Login failed.";
                }
            }
        }
        catch 
        {
            FeedbackBox.Message = $"An error occurred.";
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        FeedbackBox.Visibility = Visibility.Collapsed;
        FeedbackBox.IsSuccess = false;

        var name = RegisterFullNameTextBox.Text;
        var email = RegisterEmailTextBox.Text;
        var password = RegisterPasswordBox.Password;
        var confirmPassword = RegisterConfirmPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 70)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                FeedbackBox.Message = "Full Name is required.";
            }
            else if (name.Length < 2)
            {
                 FeedbackBox.Message = "Your full name should be at least two characters.";
            }
            else
            {
                 FeedbackBox.Message = "Full Name cannot exceed 70 characters.";
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
            FeedbackBox.Message = "Please enter a valid email address.";
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            FeedbackBox.Message = "Password is required.";
            return;
        }

        var strongPasswordAttribute = new StrongPasswordAttribute();
        if (!strongPasswordAttribute.IsValid(password))
        {
            FeedbackBox.Message = strongPasswordAttribute.ErrorMessage ?? "Password does not meet requirements.";
            return;
        }
        if (password != confirmPassword)
        {
            FeedbackBox.Message = "Passwords do not match.";
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
                FeedbackBox.IsSuccess = true;
                FeedbackBox.Message = "Your account is now pending approval by your lecturer.";
                
                RegisterFullNameTextBox.Clear();
                RegisterEmailTextBox.Clear();
                RegisterPasswordBox.Clear();
                RegisterConfirmPasswordBox.Clear();
            }
            else
            {
                FeedbackBox.Message = result.ErrorMessage ?? "Registration failed.";
            }
        }
        catch
        {
            FeedbackBox.Message = $"An error occurred.";
        }
    }

    private void ForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        var forgotPasswordWindow = ActivatorUtilities.CreateInstance<ForgotPasswordInitialWindow>(_serviceProvider);
        forgotPasswordWindow.Owner = this;
        forgotPasswordWindow.ShowDialog();
    }

    private void SignInForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LoginButton_Click(sender, e);
        }
    }

    private void RegisterForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            RegisterButton_Click(sender, e);
        }
    }
}
