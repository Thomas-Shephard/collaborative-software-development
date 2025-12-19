using Jahoot.Display.Commands;
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

        public ObservableCollection<QuestionViewModel> Questions { get; set; } = new ObservableCollection<QuestionViewModel>();

        private int _selectedNumberOfQuestions;
        public int SelectedNumberOfQuestions
        {
            get => _selectedNumberOfQuestions;
            set
            {
                if (_selectedNumberOfQuestions != value)
                {
                    _selectedNumberOfQuestions = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }


        public ICommand SaveCommand { get; }
        public ICommand DiscardCommand { get; }
        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }

        public CreateTestViewModel(ITestService testService, ISubjectService subjectService)
        {
            _testService = testService;
            _subjectService = subjectService;

            SaveCommand = new RelayCommand(async _ => await SaveTest(), CanSaveTest);
            DiscardCommand = new RelayCommand(DiscardChanges);
            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            RemoveQuestionCommand = new RelayCommand(RemoveQuestion, CanRemoveQuestion);
        }

        public async Task InitialiseAsync()
        {
            await LoadSubjects();
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

        private static bool CanRemoveQuestion(object? obj)
        {
            return obj is QuestionViewModel;
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
            return !string.IsNullOrWhiteSpace(TestName) &&
                   SelectedSubject != null &&
                   Questions.Any() &&
                   SelectedNumberOfQuestions > 0 &&
                   SelectedNumberOfQuestions <= Questions.Count &&
                   Questions.All(q => !string.IsNullOrWhiteSpace(q.QuestionText) &&
                                      q.Options.Count >= 2 && // Added validation for minimum 2 options
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

                var newTest = new Test
                {
                    Name = TestName,
                    SubjectId = SelectedSubject!.SubjectId,
                    NumberOfQuestions = SelectedNumberOfQuestions,
                    Questions = questionsToSave
                };

                var result = await _testService.CreateTest(newTest);

                if (result.Success)
                {
                    MessageBox.Show("Test created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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


        private static void DiscardChanges(object? obj)
        {
            var result = MessageBox.Show("Are you sure you want to discard changes?", "Confirm Discard", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Windows.OfType<CreateTestWindow>().FirstOrDefault()?.Close();
            }
        }
    }
}
