using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Display.Services;
using System.Windows;

namespace Jahoot.Display.AdminViews;

public partial class SubjectFormWindow : Window
{
    private readonly ISubjectService _subjectService;
    private readonly Subject? _subject;

    public SubjectFormWindow(ISubjectService subjectService, Subject? subject = null)
    {
        InitializeComponent();
        _subjectService = subjectService;
        _subject = subject;

        if (_subject != null)
        {
            Title = "Edit Subject";
            NameTextBox.Text = _subject.Name;
            ActiveCheckBox.IsChecked = _subject.IsActive;
        }
        else
        {
            Title = "Create Subject";
            ActiveCheckBox.IsChecked = true; // The user is unable to make a subject that is not active
            ActiveCheckBox.IsEnabled = false;
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

        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorText.Text = "Name is required.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        Result result = _subject == null
            ? await _subjectService.CreateSubjectAsync(new CreateSubjectRequestModel { Name = name })
            : await _subjectService.UpdateSubjectAsync(_subject.SubjectId, new UpdateSubjectRequestModel { Name = name, IsActive = ActiveCheckBox.IsChecked.GetValueOrDefault() });

        if (result.Success)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            ErrorText.Text = result.ErrorMessage ?? "An error occurred.";
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
