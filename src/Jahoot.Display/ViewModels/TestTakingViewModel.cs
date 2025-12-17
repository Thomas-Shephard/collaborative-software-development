using System.Collections.ObjectModel;
using System.Windows.Input;
using Jahoot.Core.Models;

namespace Jahoot.Display.ViewModels
{
    public class TestTakingViewModel : BaseViewModel
    {
        public string TestName
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Mathematics Test";

        public string SubjectName
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Mathematics";

        public int CurrentQuestionIndex
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(QuestionNumberText));
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(IsLastQuestion));
                LoadCurrentQuestion();
            }
        } = 0;

        public int TotalQuestions
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(QuestionNumberText));
            }
        } = 0;

        public string QuestionNumberText => $"Question {CurrentQuestionIndex + 1}/{TotalQuestions}";

        public string CurrentQuestionText
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = string.Empty;

        public ObservableCollection<QuestionOptionViewModel> CurrentOptions
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = new ObservableCollection<QuestionOptionViewModel>();

        private bool _hasAnsweredCurrentQuestion;
        public bool HasAnsweredCurrentQuestion
        {
            get => _hasAnsweredCurrentQuestion;
            set
            {
                _hasAnsweredCurrentQuestion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanGoBack => CurrentQuestionIndex > 0;

        public bool CanGoNext => CurrentQuestionIndex < TotalQuestions - 1 && HasAnsweredCurrentQuestion;

        public bool IsLastQuestion => CurrentQuestionIndex == TotalQuestions - 1;

        public string TimeRemaining
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "00:00";

        private List<Question> _questions = new();
        private Dictionary<int, int?> _selectedAnswers = new();

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SubmitCommand { get; }

        public TestTakingViewModel()
        {
            NextCommand = new RelayCommand(GoToNext, () => CanGoNext);
            BackCommand = new RelayCommand(GoToBack, () => CanGoBack);
            SubmitCommand = new RelayCommand(SubmitTest, () => HasAnsweredCurrentQuestion);

            LoadMockData();
        }

        private void LoadMockData()
        {
            _questions = new List<Question>
            {
                new Question
                {
                    QuestionId = 1,
                    Text = "What is the derivative of f(x) = 3x� + 2x - 5?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 1, QuestionId = 1, OptionText = "6x + 2", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 2, QuestionId = 1, OptionText = "3x + 2", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 3, QuestionId = 1, OptionText = "6x + 5", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 4, QuestionId = 1, OptionText = "3x� + 2", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Question
                {
                    QuestionId = 2,
                    Text = "What is the integral of f(x) = 2x?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 5, QuestionId = 2, OptionText = "x� + C", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 6, QuestionId = 2, OptionText = "2x� + C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 7, QuestionId = 2, OptionText = "x + C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 8, QuestionId = 2, OptionText = "2x + C", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new Question
                {
                    QuestionId = 3,
                    Text = "What is the value of sin(90�)?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 9, QuestionId = 3, OptionText = "1", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 10, QuestionId = 3, OptionText = "0", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 11, QuestionId = 3, OptionText = "-1", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 12, QuestionId = 3, OptionText = "0.5", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            TotalQuestions = _questions.Count;
            LoadCurrentQuestion();
        }

        private void LoadCurrentQuestion()
        {
            if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _questions.Count)
            {
                var question = _questions[CurrentQuestionIndex];
                CurrentQuestionText = question.Text;

                // Check if current question has been answered
                HasAnsweredCurrentQuestion = _selectedAnswers.ContainsKey(question.QuestionId);

                CurrentOptions.Clear();
                
                // Ensure exactly 4 options
                var options = question.Options.Take(4).ToList();
                for (int i = 0; i < options.Count; i++)
                {
                    var option = options[i];
                    var viewModel = new QuestionOptionViewModel
                    {
                        OptionText = option.OptionText,
                        OptionId = option.QuestionOptionId,
                        IsSelected = _selectedAnswers.TryGetValue(question.QuestionId, out var selectedId) && selectedId == option.QuestionOptionId
                    };
                    viewModel.SelectionChanged += OnOptionSelectionChanged;
                    CurrentOptions.Add(viewModel);
                }
            }
        }

        private void OnOptionSelectionChanged(QuestionOptionViewModel selectedOption)
        {
            var currentQuestion = _questions[CurrentQuestionIndex];
            
            foreach (var option in CurrentOptions)
            {
                if (option != selectedOption)
                {
                    option.IsSelected = false;
                }
            }

            if (selectedOption.IsSelected)
            {
                _selectedAnswers[currentQuestion.QuestionId] = selectedOption.OptionId;
                HasAnsweredCurrentQuestion = true;
            }
            else
            {
                _selectedAnswers.Remove(currentQuestion.QuestionId);
                HasAnsweredCurrentQuestion = false;
            }
        }

        private void GoToNext()
        {
            if (CanGoNext)
            {
                CurrentQuestionIndex++;
            }
        }

        private void GoToBack()
        {
            if (CanGoBack)
            {
                CurrentQuestionIndex--;
            }
        }

        private void SubmitTest()
        {
            if (!HasAnsweredCurrentQuestion)
            {
                System.Windows.MessageBox.Show("Please answer the current question before submitting.", 
                    "Answer Required", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show($"Test submitted! You answered {_selectedAnswers.Count}/{TotalQuestions} questions.", 
                "Test Submitted", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Information);
        }
    }

    public class QuestionOptionViewModel : BaseViewModel
    {
        public string OptionText
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = string.Empty;

        public int OptionId { get; set; }

        public bool IsSelected
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        SelectionChanged?.Invoke(this);
                    }
                }
            }
        }

        public event Action<QuestionOptionViewModel>? SelectionChanged;
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }
}
