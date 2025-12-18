using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Display.Services;
using System.Windows;
using Jahoot.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Jahoot.Display.AdminViews;

public partial class LecturerFormWindow : Window
{
    private readonly ILecturerService _lecturerService;
    private readonly Lecturer? _lecturer;

    public LecturerFormWindow(ILecturerService lecturerService, Lecturer? lecturer = null)
    {
        InitializeComponent();
        _lecturerService = lecturerService;
        _lecturer = lecturer;

        if (_lecturer != null)
        {
            Title = "Edit Lecturer";
            NameTextBox.Text = _lecturer.Name;
            EmailTextBox.Text = _lecturer.Email;
            IsAdminCheckBox.IsChecked = _lecturer.IsAdmin;

            // Switch to Edit Mode layout
            PasswordRow.Visibility = Visibility.Collapsed;
            ResetPasswordRow.Visibility = Visibility.Visible;
        }
        else
        {
            Title = "Create New Lecturer";
            PasswordRow.Visibility = Visibility.Visible;
            ResetPasswordRow.Visibility = Visibility.Collapsed;
            IsAdminCheckBox.IsChecked = false; // Unchecked (non-admin) by default
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        var name = NameTextBox.Text.Trim();
        var email = EmailTextBox.Text.Trim();
        var isAdmin = IsAdminCheckBox.IsChecked.GetValueOrDefault();

        // Basic Validation
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            ShowError("Name is required and must be at least 2 characters.");
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
             ShowError("Please enter a valid email address.");
             return;
        }

        Result result;

        if (_lecturer == null)
        {
            // Create Mode
            var password = PasswordBox.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Password is required.");
                return;
            }

            var strongPass = new StrongPasswordAttribute();
            if (!strongPass.IsValid(password))
            {
                ShowError(strongPass.ErrorMessage ?? "Password is too weak.");
                return;
            }

            var request = new CreateLecturerRequestModel
            {
                Name = name,
                Email = email,
                Password = password,
                IsAdmin = isAdmin
            };

            result = await _lecturerService.CreateLecturerAsync(request);
        }
        else
        {
            // Edit Mode
            var request = new UpdateLecturerRequestModel
            {
                Name = name,
                Email = email,
                IsAdmin = isAdmin,
                IsDisabled = _lecturer.IsDisabled
            };

            result = await _lecturerService.UpdateLecturerAsync(_lecturer.UserId, request);
        }

        if (result.Success)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            ShowError(result.ErrorMessage ?? "An error occurred.");
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private async void ResetPassword_Click(object sender, RoutedEventArgs e)
    {
        if (_lecturer == null) return;

        var result = MessageBox.Show($"Are you sure you want to reset the password for {_lecturer.Name}?\nThis will send a password reset email to {_lecturer.Email}.",
            "Confirm Password Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var resetResult = await _lecturerService.ResetLecturerPasswordAsync(_lecturer.Email);
                if (resetResult.Success)
                {
                    MessageBox.Show("Password reset email has been sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ShowError(resetResult.ErrorMessage ?? "Failed to send reset email.");
                }
            }
            catch
            {
                ShowError($"An error occurred.");
            }
        }
    }
}
