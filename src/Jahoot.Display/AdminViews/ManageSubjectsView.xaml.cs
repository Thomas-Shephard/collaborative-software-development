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
            await LoadSubjects(true);
        }
    }

    public void Initialize(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    private async Task LoadSubjects(bool forceUpdateStats = false)
    {
        if (_subjectService == null) return;

        try
        {
            bool shouldUpdateStats = forceUpdateStats || SelectedFilter == "All";
            List<Subject> subjectsForGrid;

            if (shouldUpdateStats)
            {
                var allSubjects = (await _subjectService.GetAllSubjectsAsync(null)).ToList();

                TotalSubjects = allSubjects.Count;
                ActiveSubjects = allSubjects.Count(s => s.IsActive);
                InactiveSubjects = allSubjects.Count(s => !s.IsActive);

                subjectsForGrid = SelectedFilter switch
                {
                    "Active" => allSubjects.Where(s => s.IsActive).ToList(),
                    "Inactive" => allSubjects.Where(s => !s.IsActive).ToList(),
                    _ => allSubjects
                };
            }
            else
            {
                bool? isActive = SelectedFilter switch
                {
                    "Active" => true,
                    "Inactive" => false,
                    _ => null
                };
                subjectsForGrid = [.. (await _subjectService.GetAllSubjectsAsync(isActive))];
            }

            Subjects.Clear();
            foreach (Subject subject in subjectsForGrid)
            {
                Subjects.Add(subject);
            }
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
            await LoadSubjects(true);
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadSubjects(true);
    }

    private async void EditSubject_Click(object sender, RoutedEventArgs e)
    {
        if (_subjectService == null) return;

        if (sender is Button button && button.CommandParameter is Subject subject)
        {
            SubjectFormWindow form = new(_subjectService, subject);
            if (form.ShowDialog() == true)
            {
                await LoadSubjects(true);
            }
        }
    }
}
