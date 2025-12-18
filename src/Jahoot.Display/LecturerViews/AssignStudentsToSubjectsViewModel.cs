using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Jahoot.Display.LecturerViews
{
    public class AssignStudentsToSubjectsViewModel : BaseViewModel
    {
        private readonly ISubjectService _subjectService;
        private readonly IStudentService _studentService;

        private ObservableCollection<Subject> _subjects = [];
        public ObservableCollection<Subject> Subjects
        {
            get => _subjects;
            set { _subjects = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Student> _students = [];
        public ObservableCollection<Student> Students
        {
            get => _students;
            set { _students = value; OnPropertyChanged(); }
        }

        private Subject? _selectedSubject;
        public Subject? SelectedSubject
        {
            get => _selectedSubject;
            set { _selectedSubject = value; OnPropertyChanged(); }
        }

        private string _feedbackMessage = string.Empty;
        public string FeedbackMessage
        {
            get => _feedbackMessage;
            set { _feedbackMessage = value; OnPropertyChanged(); }
        }

        private bool _isFeedbackSuccess;
        public bool IsFeedbackSuccess
        {
            get => _isFeedbackSuccess;
            set { _isFeedbackSuccess = value; OnPropertyChanged(); }
        }

        public ICommand AssignCommand { get; }

        public AssignStudentsToSubjectsViewModel(ISubjectService subjectService, IStudentService studentService)
        {
            _subjectService = subjectService;
            _studentService = studentService;
            AssignCommand = new RelayCommand(AssignStudents);
            _ = LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var subjectsTask = _subjectService.GetAllSubjectsAsync(isActive: true);
                var approvedTask = _studentService.GetStudents(true);
                var unapprovedTask = _studentService.GetStudents(false);

                await Task.WhenAll(subjectsTask, approvedTask, unapprovedTask);

                Subjects = new ObservableCollection<Subject>(subjectsTask.Result);

                var allStudents = approvedTask.Result.Concat(unapprovedTask.Result).OrderBy(s => s.Name);
                Students = new ObservableCollection<Student>(allStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AssignStudents(object? parameter)
        {
            FeedbackMessage = string.Empty;

            if (SelectedSubject == null)
            {
                IsFeedbackSuccess = false;
                FeedbackMessage = "Please select a subject first.";
                return;
            }

            if (parameter is System.Windows.Controls.ListBox listBox && listBox.SelectedItems.Count > 0)
            {
                var studentsToAssign = listBox.SelectedItems.Cast<Student>().ToList();
                int successCount = 0;
                int failCount = 0;

                try
                {
                    foreach (var student in studentsToAssign)
                    {
                        var currentSubjects = student.Subjects?.ToList() ?? new List<Subject>();
                        
                        // Check if already assigned
                        if (currentSubjects.Any(s => s.SubjectId == SelectedSubject.SubjectId))
                        {
                            continue; // Already assigned
                        }

                        currentSubjects.Add(SelectedSubject);

                        // Create a separate student object to pass to update service,
                        // so we don't mutate the original instance from the collection.
                        var updatedStudent = new Student
                        {
                            UserId = student.UserId,
                            Subjects = currentSubjects
                        };

                        var result = await _studentService.UpdateStudent(student.UserId, updatedStudent);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }

                    if (successCount > 0 || failCount > 0)
                    {
                        if (failCount > 0)
                        {
                            IsFeedbackSuccess = false;
                            FeedbackMessage = $"Assigned {successCount} students. Failed to assign {failCount}.";
                        }
                        else
                        {
                            IsFeedbackSuccess = true;
                            FeedbackMessage = $"Successfully assigned {successCount} students to {SelectedSubject.Name}.";
                        }
                        
                        listBox.UnselectAll();

                        // Refresh data to ensure UI is in sync
                        await LoadData();
                    }
                    else
                    {
                         // Case where all selected students were already assigned
                         IsFeedbackSuccess = true;
                         FeedbackMessage = "Selected students are already assigned to this subject.";
                         listBox.UnselectAll();
                    }
                }
                catch (Exception ex)
                {
                    IsFeedbackSuccess = false;
                    FeedbackMessage = $"Error during assignment: {ex.Message}";
                }
            }
            else
            {
                IsFeedbackSuccess = false;
                FeedbackMessage = "Please select at least one student.";
            }
        }
    }
}