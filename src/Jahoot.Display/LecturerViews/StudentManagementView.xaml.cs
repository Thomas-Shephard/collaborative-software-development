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
                var approvedStudents = await _studentService.GetStudents(true);
                var unapprovedStudents = await _studentService.GetStudents(false);
                var allStudents = approvedStudents.Concat(unapprovedStudents);
                Students = new ObservableCollection<Student>(allStudents);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading students: {ex.Message}");
            }
        }

        private async void EditStudent(object? obj)
        {
            if (obj is Student student)
            {
                var editWindow = new EditStudentWindow(student);
                if (editWindow.ShowDialog() == true)
                {
                    await _studentService.UpdateStudent(student.UserId, student);
                    await LoadStudents();
                }
            }
        }

        private async void ApproveStudent(object? obj)
        {
            if (obj is Student student)
            {
                student.IsApproved = true;
                student.IsDisabled = false;
                await _studentService.UpdateStudent(student.UserId, student);
                await LoadStudents();
            }
        }

        private async void RejectStudent(object? obj)
        {
            if (obj is Student studentToReject)
            {
                // The API prevents unapproving an already approved student.
                // To maintain the existing local behaviour of removing "rejected" students,
                // we will delete the student if they are not already approved.
                // If they are approved, we might consider disabling them instead,
                // but for now, matching the current local logic (removal)
                // for unapproved students means deletion.
                if (!studentToReject.IsApproved)
                {
                    await _studentService.DeleteStudent(studentToReject.UserId);
                    await LoadStudents();
                }
                else
                {
                    // If an approved student is "rejected", perhaps disable them?
                    // Or show a message that approved students cannot be unapproved.
                    Debug.WriteLine($"Approved student {studentToReject.Name} cannot be unapproved.");
                    // For now, doing nothing for approved students that are "rejected"
                    // to avoid unintended data loss or API errors.
                }
            }
        }

        private async void DeleteStudent(object? obj)
        {
            if (obj is Student studentToDelete)
            {
                await _studentService.DeleteStudent(studentToDelete.UserId);
                await LoadStudents();
            }
        }

        private async void EnableStudent(object? obj)
        {
            if (obj is Student student)
            {
                student.IsDisabled = false;
                await _studentService.UpdateStudent(student.UserId, student);
                await LoadStudents();
            }
        }

        private async void DisableStudent(object? obj)
        {
            if (obj is Student student)
            {
                student.IsDisabled = true;
                await _studentService.UpdateStudent(student.UserId, student);
                await LoadStudents();
            }
        }
    }
}
