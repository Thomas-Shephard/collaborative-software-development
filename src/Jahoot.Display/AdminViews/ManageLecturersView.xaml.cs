using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.AdminViews;

public partial class ManageLecturersView : UserControl, INotifyPropertyChanged
{
    private ILecturerService _lecturerService = null!;
    private ObservableCollection<Lecturer> _lecturers = [];

    public ObservableCollection<Lecturer> Lecturers
    {
        get => _lecturers;
        set
        {
            _lecturers = value;
            OnPropertyChanged(nameof(Lecturers));
        }
    }

    public ManageLecturersView()
    {
        InitializeComponent();
        DataContext = this;
    }

    public void Initialize(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
        LoadLecturers();
    }

    private async void LoadLecturers()
    {
        try
        {
            var lecturers = await _lecturerService.GetAllLecturersAsync();
            Lecturers = new ObservableCollection<Lecturer>(lecturers);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading lecturers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadLecturers();
    }

    private void CreateLecturer_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Create Lecturer feature is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void EditLecturer_Click(object sender, RoutedEventArgs e)
    {
        var lecturer = (sender as Button)?.CommandParameter as Lecturer;
        if (lecturer == null) return;

        MessageBox.Show($"Edit feature for {lecturer.Name} is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DisableLecturer_Click(object sender, RoutedEventArgs e)
    {
        var lecturer = (sender as Button)?.CommandParameter as Lecturer;
        if (lecturer == null) return;

        MessageBox.Show($"Disable feature for {lecturer.Name} is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void DeleteLecturer_Click(object sender, RoutedEventArgs e)
    {
        var lecturer = (sender as Button)?.CommandParameter as Lecturer;
        if (lecturer == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete {lecturer.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
             try
             {
                 var deleteResult = await _lecturerService.DeleteLecturerAsync(lecturer.UserId); // Assuming UserId is the ID to use
                 if (deleteResult.Success)
                 {
                     LoadLecturers();
                 }
                 else
                 {
                     MessageBox.Show($"Error deleting lecturer: {deleteResult.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Error deleting lecturer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
             }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
