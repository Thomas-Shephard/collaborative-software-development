using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Display.Services;
using System.Windows;
using Jahoot.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Jahoot.Display.AdminViews;

public partial class AssignLecturerRoleWindow : Window
{
    private readonly ILecturerService _lecturerService;

    public AssignLecturerRoleWindow(ILecturerService lecturerService)
    {
        InitializeComponent();
        _lecturerService = lecturerService;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        var email = EmailTextBox.Text.Trim();
        var isAdmin = IsAdminCheckBox.IsChecked.GetValueOrDefault();

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
             ShowError("Please enter a valid email address.");
             return;
        }

        var request = new AssignLecturerRoleRequestModel
        {
            Email = email,
            IsAdmin = isAdmin
        };

        var result = await _lecturerService.AssignLecturerRoleAsync(request);

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
}
