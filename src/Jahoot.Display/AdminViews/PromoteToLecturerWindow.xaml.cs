using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Display.Services;
using System.Windows;
using Jahoot.Core.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace Jahoot.Display.AdminViews;

public partial class PromoteToLecturerWindow : Window
{
    private readonly ILecturerService _lecturerService;

    public PromoteToLecturerWindow(ILecturerService lecturerService)
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
        FeedbackBox.Visibility = Visibility.Collapsed;
        FeedbackBox.IsSuccess = false;
        var email = EmailTextBox.Text.Trim();
        var isAdmin = IsAdminCheckBox.IsChecked.GetValueOrDefault();

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
             FeedbackBox.Message = "Please enter a valid email address.";
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
             FeedbackBox.Message = result.ErrorMessage ?? "An error occurred.";
        }
    }

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Save_Click(sender, e);
        }
    }
}