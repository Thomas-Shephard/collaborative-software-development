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
    public class EditTestViewModel : BaseViewModel
    {
        private readonly ITestService _testService;
        private readonly ISubjectService _subjectService;
        private readonly Test _originalTest; // To hold the original test data

        private int _testId;
        public int TestId
        {
            get => _testId;
            set
            {
                _testId = value;
                OnPropertyChanged();
            }
        }

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

        public ObservableCollection<QuestionViewModel> Questions { get; set; } = new ObservableCollection<QuestionViewModel>();

        public ICommand SaveCommand { get; }
        public ICommand DiscardCommand { get; }
        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }

        public EditTestViewModel(ITestService testService, ISubjectService subjectService, Test testToEdit)
        {
            _testService = testService;
            _subjectService = subjectService;
            _originalTest = testToEdit; // Store the original test

            SaveCommand = new RelayCommand(async _ => await SaveTest(), CanSaveTest);
            DiscardCommand = new RelayCommand(DiscardChanges);
            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            RemoveQuestionCommand = new RelayCommand(RemoveQuestion, CanRemoveQuestion);

            _ = LoadSubjects();
            LoadTestToEdit(testToEdit);
        }

        private void LoadTestToEdit(Test test)
        {
            TestId = test.TestId;
            TestName = test.Name;
            // SelectedSubject will be set when subjects are loaded
            
            Questions.Clear();
            foreach (var question in test.Questions)
            {
                var questionVm = new QuestionViewModel
                {
                    QuestionText = question.Text
                };
                foreach (var option in question.Options)
                {
                    questionVm.Options.Add(new QuestionOptionViewModel
                    {
                        OptionText = option.OptionText,
                        IsCorrect = option.IsCorrect
                    });
                }
                Questions.Add(questionVm);
            }
        }

        private void AddQuestion()
        {
            Questions.Add(new QuestionViewModel());
            CommandManager.InvalidateRequerySuggested();
        }

        private void RemoveQuestion(object? obj)
        {
            if (obj is QuestionViewModel question)
            {
                Questions.Remove(question);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool CanRemoveQuestion(object? obj)
        {
            return obj is QuestionViewModel;
        }

        private async Task LoadSubjects()
        {
            try
            {
                var subjects = await _subjectService.GetSubjects();
                Subjects = new ObservableCollection<Subject>(subjects);
                // Set SelectedSubject after subjects are loaded
                SelectedSubject = Subjects.FirstOrDefault(s => s.SubjectId == _originalTest.SubjectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading subjects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveTest(object? arg)
        {
            return !string.IsNullOrWhiteSpace(TestName) &&
                   SelectedSubject != null &&
                   Questions.Any() &&
                   Questions.All(q => !string.IsNullOrWhiteSpace(q.QuestionText) &&
                                      q.Options.Any(o => !string.IsNullOrWhiteSpace(o.OptionText)) &&
                                      q.Options.Count(o => o.IsCorrect) == 1);
        }

        private async Task SaveTest()
        {
            try
            {
                var questionsToSave = Questions.Select(qvm => new Question
                {
                    Text = qvm.QuestionText,
                    Options = qvm.Options.Select(ovm => new QuestionOption
                    {
                        OptionText = ovm.OptionText,
                        IsCorrect = ovm.IsCorrect
                    }).ToList().AsReadOnly()
                }).ToList().AsReadOnly();

                var updatedTest = new Test
                {
                    TestId = TestId,
                    Name = TestName,
                    SubjectId = SelectedSubject!.SubjectId,
                    NumberOfQuestions = questionsToSave.Count,
                    Questions = questionsToSave
                };
                Debug.WriteLine($"Attempting to update test with TestId: {TestId}");
                var result = await _testService.UpdateTest(TestId, updatedTest);

                if (result.Success)
                {
                    MessageBox.Show("Test updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Windows.OfType<EditTestWindow>().FirstOrDefault()?.Close();
                }
                else
                {
                    MessageBox.Show($"Failed to update test: {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DiscardChanges(object? obj)
        {
            var result = MessageBox.Show("Are you sure you want to discard changes?", "Confirm Discard", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Windows.OfType<EditTestWindow>().FirstOrDefault()?.Close();
            }
        }
    }
}
