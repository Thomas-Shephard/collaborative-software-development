using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.AdminViews;

public partial class ManageSubjectsView : UserControl, INotifyPropertyChanged
{
    private ISubjectService? _subjectService;
    public ObservableCollection<Subject> Subjects { get; set; } = [];

    public int TotalSubjects
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int ActiveSubjects
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int InactiveSubjects
    {
        get => field;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public List<string> FilterOptions { get; } = ["Active", "Inactive", "All"];

    public string SelectedFilter
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                _ = LoadSubjects();
            }
        }
    } = "All";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ManageSubjectsView()
    {
        InitializeComponent();
        DataContext = this;
        IsVisibleChanged += ManageSubjectsView_IsVisibleChanged;
    }

    private async void ManageSubjectsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            await LoadSubjects();
        }
    }

    public void Initialize(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    private async Task LoadSubjects()
    {
        if (_subjectService == null) return;

        try
        {
            bool? isActive = SelectedFilter switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            List<Subject> subjects = [.. (await _subjectService.GetAllSubjectsAsync(isActive))];

            Subjects.Clear();
            foreach (Subject subject in subjects)
            {
                Subjects.Add(subject);
            }

            TotalSubjects = subjects.Count;
            ActiveSubjects = subjects.Count(s => s.IsActive);
            InactiveSubjects = subjects.Count(s => !s.IsActive);
        }
        catch
        {
            MessageBox.Show("Failed to load subjects.");
        }
    }

    private async void CreateSubject_Click(object sender, RoutedEventArgs e)
    {
        if (_subjectService == null) return;

        SubjectFormWindow form = new(_subjectService);
        if (form.ShowDialog() == true)
        {
            await LoadSubjects();
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadSubjects();
    }

    private async void EditSubject_Click(object sender, RoutedEventArgs e)
    {
        if (_subjectService == null) return;

        if (sender is Button button && button.CommandParameter is Subject subject)
        {
            SubjectFormWindow form = new(_subjectService, subject);
            if (form.ShowDialog() == true)
            {
                await LoadSubjects();
            }
        }
    }
}
