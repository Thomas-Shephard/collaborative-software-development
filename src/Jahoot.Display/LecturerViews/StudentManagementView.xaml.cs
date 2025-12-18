using Jahoot.Core.Models;
using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Jahoot.Display.LecturerViews
{

    public partial class StudentManagementView : UserControl
    {
        public StudentManagementView()
        {
            InitializeComponent();
            DataContext = ((App)App.Current).ServiceProvider.GetRequiredService<StudentManagementViewModel>();
        }
    }

    public class StudentManagementViewModel : BaseViewModel
    {
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();
        public ObservableCollection<Student> Students
        {
            get { return _students; }
            set
            {
                _students = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditStudentCommand { get; }
        public ICommand ApproveStudentCommand { get; }
        public ICommand RejectStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand EnableStudentCommand { get; }
        public ICommand DisableStudentCommand { get; }


        private readonly IStudentService _studentService;
        public StudentManagementViewModel(IStudentService studentService)
        {
            _studentService = studentService;

            EditStudentCommand = new RelayCommand(EditStudent);
            ApproveStudentCommand = new RelayCommand(ApproveStudent);
            RejectStudentCommand = new RelayCommand(RejectStudent);
            DeleteStudentCommand = new RelayCommand(DeleteStudent);
            EnableStudentCommand = new RelayCommand(EnableStudent);
            DisableStudentCommand = new RelayCommand(DisableStudent);

            _ = LoadStudents();
        }

        private async Task LoadStudents()
        {
            try
            {
                var approvedTask = _studentService.GetStudents(true);
                var unapprovedTask = _studentService.GetStudents(false);
                await Task.WhenAll(approvedTask, unapprovedTask);
                var approvedStudents = approvedTask.Result;
                var unapprovedStudents = unapprovedTask.Result;

                var allStudents = approvedStudents.Concat(unapprovedStudents);
                Students = new ObservableCollection<Student>(allStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while loading students. Please try again or contact support if the problem persists." +
                    Environment.NewLine + Environment.NewLine +
                    $"Details: {ex.Message}",
                    "Error Loading Students",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void EditStudent(object? obj)
        {
            if (obj is Student student)
            {
                try
                {
                    var editWindow = new EditStudentWindow(student);
                    if (editWindow.ShowDialog() == true)
                    {
                        await _studentService.UpdateStudent(student.UserId, student);
                        await LoadStudents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating student: {ex.Message}");
                    MessageBox.Show($"An error occurred while updating the student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ApproveStudent(object? obj)
        {
            if (obj is Student student)
            {
                try
                {
                    var result = MessageBox.Show($"Are you sure you want to approve {student.Name}?", "Confirm Approval", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        student.IsApproved = true;
                        student.IsDisabled = false;
                        await _studentService.UpdateStudent(student.UserId, student);
                        await LoadStudents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error approving student: {ex.Message}");
                    MessageBox.Show($"An error occurred while approving the student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RejectStudent(object? obj)
        {
            if (obj is Student studentToReject)
            {
                try
                {
                    if (!studentToReject.IsApproved)
                    {
                        var result = MessageBox.Show($"Are you sure you want to reject {studentToReject.Name}?", "Confirm Rejection", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            await _studentService.DeleteStudent(studentToReject.UserId);
                            await LoadStudents();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error rejecting student: {ex.Message}");
                    MessageBox.Show($"An error occurred while rejecting the student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteStudent(object? obj)
        {
            if (obj is Student studentToDelete)
            {
                try
                {
                    var result = MessageBox.Show($"Are you sure you want to delete {studentToDelete.Name}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        await _studentService.DeleteStudent(studentToDelete.UserId);
                        await LoadStudents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting student: {ex.Message}");
                    MessageBox.Show($"An error occurred while deleting the student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EnableStudent(object? obj)
        {
            if (obj is Student student)
            {
                try
                {
                    var result = MessageBox.Show($"Are you sure you want to enable {student.Name}?", "Confirm Enable", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        student.IsDisabled = false;
                        await _studentService.UpdateStudent(student.UserId, student);
                        await LoadStudents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error enabling student: {ex.Message}");
                    MessageBox.Show($"An error occurred while enabling the student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DisableStudent(object? obj)
        {
            if (obj is Student student)
            {
                try
                {
                    var result = MessageBox.Show($"Are you sure you want to disable {student.Name}?", "Confirm Disable", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        student.IsDisabled = true;
                        await _studentService.UpdateStudent(student.UserId, student);
                        await LoadStudents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disabling student: {ex.Message}");
                    MessageBox.Show($"An error occurred while disabling the student: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
