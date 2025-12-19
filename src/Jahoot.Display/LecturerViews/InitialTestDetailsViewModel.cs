using Jahoot.Core.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Jahoot.Display.Commands;

namespace Jahoot.Display.LecturerViews
{
    public class InitialTestDetailsViewModel : BaseViewModel
    {
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

        public ICommand NextCommand { get; }

        public Action? ProceedToQuestionCreation { get; set; }

        public InitialTestDetailsViewModel(ISubjectService subjectService)
        {
            _subjectService = subjectService;

            NextCommand = new RelayCommand(ExecuteNextCommand, CanExecuteNextCommand);
        }

        public async Task InitialiseAsync()
        {
            await LoadSubjects();
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

        private bool CanExecuteNextCommand(object? arg)
        {
            return !string.IsNullOrWhiteSpace(TestName) && SelectedSubject != null && NumberOfQuestions > 0;
        }

        private void ExecuteNextCommand(object? obj)
        {
            ProceedToQuestionCreation?.Invoke();
        }
    }
}
