using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Display.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Extensions;

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
        IsVisibleChanged += ManageLecturersView_IsVisibleChanged;
    }

    public async Task Initialize(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
        await LoadLecturers();
    }

    private async void ManageLecturersView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible && _lecturerService != null && (Lecturers == null || Lecturers.Count == 0))
        {
            await LoadLecturers();
        }
    }
    private async Task LoadLecturers()
    {
        try
        {
            var lecturers = await _lecturerService.GetAllLecturersAsync();
            Lecturers.UpdateCollection(lecturers);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading lecturers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadLecturers();
    }

    private async void CreateLecturer_Click(object sender, RoutedEventArgs e)
    {
        var form = new LecturerFormWindow(_lecturerService);
        form.Owner = Window.GetWindow(this);
        if (form.ShowDialog() == true)
        {
            await LoadLecturers();
        }
    }

    private async void AssignLecturer_Click(object sender, RoutedEventArgs e)
    {
        var form = new AssignLecturerRoleWindow(_lecturerService);
        form.Owner = Window.GetWindow(this);
        if (form.ShowDialog() == true)
        {
            await LoadLecturers();
        }
    }

    private async void EditLecturer_Click(object sender, RoutedEventArgs e)
    {
        var lecturer = (sender as Button)?.CommandParameter as Lecturer;
        if (lecturer == null) return;

        var form = new LecturerFormWindow(_lecturerService, lecturer);
        form.Owner = Window.GetWindow(this);
        if (form.ShowDialog() == true)
        {
            await LoadLecturers();
        }
    }

    private async void DisableLecturer_Click(object sender, RoutedEventArgs e)
    {
        var lecturer = (sender as Button)?.CommandParameter as Lecturer;
        if (lecturer == null) return;

        string action = lecturer.IsDisabled ? "enable" : "disable";
        var result = MessageBox.Show($"Are you sure you want to {action} this account?", $"Confirm {char.ToUpper(action[0]) + action.Substring(1)}", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var request = new UpdateLecturerRequestModel
                {
                    Name = lecturer.Name,
                    Email = lecturer.Email,
                    IsAdmin = lecturer.IsAdmin,
                    IsDisabled = !lecturer.IsDisabled
                };

                var updateResult = await _lecturerService.UpdateLecturerAsync(lecturer.UserId, request);
                if (updateResult.Success)
                {
                    await LoadLecturers();
                }
                else
                {
                    MessageBox.Show($"Error updating lecturer status: {updateResult.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show($"Error updating lecturer status.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
            
                private async void DeleteLecturer_Click(object sender, RoutedEventArgs e)
                {
                    var lecturer = (sender as Button)?.CommandParameter as Lecturer;
                    if (lecturer == null) return;
            
                    var result = MessageBox.Show($"Warning: This action is permanent. Are you sure you want to delete this account?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                         try
                         {
                             var deleteResult = await _lecturerService.DeleteLecturerAsync(lecturer.UserId);
                             if (deleteResult.Success)
                             {
                                 await LoadLecturers();
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
