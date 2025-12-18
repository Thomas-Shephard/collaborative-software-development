using Jahoot.Core.Models;
using Jahoot.Display.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace Jahoot.Display.LecturerViews
{
    public class CreateTestViewModel : BaseViewModel
    {
        private readonly ITestService _testService;
        private readonly ISubjectService _subjectService;

        private string _testName = string.Empty;
        public string TestName
        {
            get => _testName;
            set
            {
                _testName = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private ObservableCollection<Subject> _subjects = new ObservableCollection<Subject>();
        public ObservableCollection<Subject> Subjects
        {
            get => _subjects;
            set
            {
                _subjects = value;
                OnPropertyChanged();
            }
        }

        private Subject? _selectedSubject;
        public Subject? SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _numberOfQuestions;
        public int NumberOfQuestions
        {
            get => _numberOfQuestions;
            set
            {
                _numberOfQuestions = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand DiscardCommand { get; }

        public CreateTestViewModel(ITestService testService, ISubjectService subjectService)
        {
            _testService = testService;
            _subjectService = subjectService;

            SaveCommand = new RelayCommand(async _ => await SaveTest(), CanSaveTest);
            DiscardCommand = new RelayCommand(DiscardChanges);

            _ = LoadSubjects();
        }

        private async Task LoadSubjects()
        {
            try
            {
                var subjects = await _subjectService.GetSubjects();
                Subjects = new ObservableCollection<Subject>(subjects);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading subjects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveTest(object? arg)
        {
            return !string.IsNullOrWhiteSpace(TestName) && SelectedSubject != null && NumberOfQuestions > 0;
        }

        private async Task SaveTest()
        {
            try
            {
                var newTest = new Test
                {
                    Name = TestName,
                    SubjectId = SelectedSubject!.SubjectId,
                    NumberOfQuestions = NumberOfQuestions, // This is where the value is taken from
                    Questions = new List<Question>().AsReadOnly() // No questions are added during creation from this view
                };
                Debug.WriteLine($"Attempting to create test with NumberOfQuestions: {NumberOfQuestions}");
                var result = await _testService.CreateTest(newTest);

                if (result.Success)
                {
                    MessageBox.Show("Test created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Close the window
                    Application.Current.Windows.OfType<CreateTestWindow>().FirstOrDefault()?.Close();
                }
                else
                {
                    MessageBox.Show($"Failed to create test: {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while creating the test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DiscardChanges(object? obj)
        {
            var result = MessageBox.Show("Are you sure you want to discard changes?", "Confirm Discard", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Windows.OfType<CreateTestWindow>().FirstOrDefault()?.Close();
            }
        }
    }
}
