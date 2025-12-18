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
                OnPropertyChanged(nameof(CanSubmit));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanGoBack => CurrentQuestionIndex > 0;

        public bool CanGoNext => CurrentQuestionIndex < TotalQuestions - 1 && HasAnsweredCurrentQuestion;

        public bool IsLastQuestion => CurrentQuestionIndex == TotalQuestions - 1;

        public bool CanSubmit => _selectedAnswers.Count == TotalQuestions;

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
        private readonly DateTime _testStartTime;

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SubmitCommand { get; }

        // Event to notify when test is submitted and window should close
        public event EventHandler? TestSubmitted;

        public TestTakingViewModel()
        {
            _testStartTime = DateTime.Now;
            NextCommand = new RelayCommand(GoToNext, () => CanGoNext);
            BackCommand = new RelayCommand(GoToBack, () => CanGoBack);
            SubmitCommand = new RelayCommand(SubmitTest, () => CanSubmit);
        }

        /// <summary>
        /// Loads a mock test with 5 questions based on the test ID
        /// </summary>
        public void LoadMockTest(int testId, string testName, string subjectName)
        {
            TestName = testName;
            SubjectName = subjectName;
            _selectedAnswers.Clear();
            CurrentQuestionIndex = 0;

            _questions = testId switch
            {
                1 => GetMathematicsQuestions(),
                2 => GetChemistryQuestions(),
                3 => GetProgrammingQuestions(),
                _ => GetDefaultQuestions()
            };

            TotalQuestions = _questions.Count;
            LoadCurrentQuestion();
        }

        private List<Question> GetMathematicsQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    QuestionId = 1,
                    Text = "What is the derivative of f(x) = 3x² + 2x - 5?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 1, QuestionId = 1, OptionText = "6x + 2", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 2, QuestionId = 1, OptionText = "3x + 2", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 3, QuestionId = 1, OptionText = "6x + 5", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 4, QuestionId = 1, OptionText = "3x² + 2", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 2,
                    Text = "What is the integral of f(x) = 2x?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 5, QuestionId = 2, OptionText = "x² + C", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 6, QuestionId = 2, OptionText = "2x² + C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 7, QuestionId = 2, OptionText = "x + C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 8, QuestionId = 2, OptionText = "2x + C", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 3,
                    Text = "What is the value of sin(90°)?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 9, QuestionId = 3, OptionText = "1", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 10, QuestionId = 3, OptionText = "0", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 11, QuestionId = 3, OptionText = "-1", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 12, QuestionId = 3, OptionText = "0.5", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 4,
                    Text = "What is 2³?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 13, QuestionId = 4, OptionText = "8", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 14, QuestionId = 4, OptionText = "6", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 15, QuestionId = 4, OptionText = "9", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 16, QuestionId = 4, OptionText = "4", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 5,
                    Text = "What is the area of a circle with radius 3?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 17, QuestionId = 5, OptionText = "9?", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 18, QuestionId = 5, OptionText = "6?", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 19, QuestionId = 5, OptionText = "3?", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 20, QuestionId = 5, OptionText = "12?", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                }
            };
        }

        private List<Question> GetChemistryQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    QuestionId = 1,
                    Text = "What is the chemical symbol for Gold?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 1, QuestionId = 1, OptionText = "Au", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 2, QuestionId = 1, OptionText = "Ag", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 3, QuestionId = 1, OptionText = "Go", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 4, QuestionId = 1, OptionText = "Gd", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 2,
                    Text = "What is H?O commonly known as?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 5, QuestionId = 2, OptionText = "Water", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 6, QuestionId = 2, OptionText = "Oxygen", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 7, QuestionId = 2, OptionText = "Hydrogen Peroxide", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 8, QuestionId = 2, OptionText = "Acid", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 3,
                    Text = "What is the pH of a neutral solution?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 9, QuestionId = 3, OptionText = "7", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 10, QuestionId = 3, OptionText = "0", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 11, QuestionId = 3, OptionText = "14", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 12, QuestionId = 3, OptionText = "1", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 4,
                    Text = "What is the atomic number of Carbon?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 13, QuestionId = 4, OptionText = "6", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 14, QuestionId = 4, OptionText = "12", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 15, QuestionId = 4, OptionText = "8", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 16, QuestionId = 4, OptionText = "14", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 5,
                    Text = "What gas do plants absorb from the atmosphere?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 17, QuestionId = 5, OptionText = "Carbon Dioxide", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 18, QuestionId = 5, OptionText = "Oxygen", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 19, QuestionId = 5, OptionText = "Nitrogen", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 20, QuestionId = 5, OptionText = "Hydrogen", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                }
            };
        }

        private List<Question> GetProgrammingQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    QuestionId = 1,
                    Text = "What does OOP stand for?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 1, QuestionId = 1, OptionText = "Object-Oriented Programming", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 2, QuestionId = 1, OptionText = "Open Operating Protocol", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 3, QuestionId = 1, OptionText = "Optimal Output Processing", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 4, QuestionId = 1, OptionText = "Organized Object Procedure", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 2,
                    Text = "Which data structure uses LIFO?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 5, QuestionId = 2, OptionText = "Stack", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 6, QuestionId = 2, OptionText = "Queue", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 7, QuestionId = 2, OptionText = "Array", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 8, QuestionId = 2, OptionText = "Linked List", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 3,
                    Text = "What is the time complexity of binary search?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 9, QuestionId = 3, OptionText = "O(log n)", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 10, QuestionId = 3, OptionText = "O(n)", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 11, QuestionId = 3, OptionText = "O(n²)", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 12, QuestionId = 3, OptionText = "O(1)", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 4,
                    Text = "Which sorting algorithm is generally fastest?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 13, QuestionId = 4, OptionText = "Quick Sort", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 14, QuestionId = 4, OptionText = "Bubble Sort", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 15, QuestionId = 4, OptionText = "Selection Sort", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 16, QuestionId = 4, OptionText = "Insertion Sort", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 5,
                    Text = "What is encapsulation in OOP?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 17, QuestionId = 5, OptionText = "Hiding internal details", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 18, QuestionId = 5, OptionText = "Creating multiple instances", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 19, QuestionId = 5, OptionText = "Sorting data", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 20, QuestionId = 5, OptionText = "Looping through arrays", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                }
            };
        }

        private List<Question> GetDefaultQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    QuestionId = 1,
                    Text = "Sample Question 1?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 1, QuestionId = 1, OptionText = "Option A", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 2, QuestionId = 1, OptionText = "Option B", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 3, QuestionId = 1, OptionText = "Option C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 4, QuestionId = 1, OptionText = "Option D", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 2,
                    Text = "Sample Question 2?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 5, QuestionId = 2, OptionText = "Option A", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 6, QuestionId = 2, OptionText = "Option B", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 7, QuestionId = 2, OptionText = "Option C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 8, QuestionId = 2, OptionText = "Option D", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 3,
                    Text = "Sample Question 3?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 9, QuestionId = 3, OptionText = "Option A", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 10, QuestionId = 3, OptionText = "Option B", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 11, QuestionId = 3, OptionText = "Option C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 12, QuestionId = 3, OptionText = "Option D", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 4,
                    Text = "Sample Question 4?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 13, QuestionId = 4, OptionText = "Option A", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 14, QuestionId = 4, OptionText = "Option B", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 15, QuestionId = 4, OptionText = "Option C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 16, QuestionId = 4, OptionText = "Option D", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 5,
                    Text = "Sample Question 5?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 17, QuestionId = 5, OptionText = "Option A", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 18, QuestionId = 5, OptionText = "Option B", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 19, QuestionId = 5, OptionText = "Option C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 20, QuestionId = 5, OptionText = "Option D", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                }
            };
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
            
            OnPropertyChanged(nameof(CanSubmit));
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
            if (_selectedAnswers.Count < TotalQuestions)
            {
                System.Windows.MessageBox.Show($"Please answer all questions before submitting. You have answered {_selectedAnswers.Count}/{TotalQuestions} questions.", 
                    "Incomplete Test", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"Test '{TestName}' submitted!\n\nYou answered all {TotalQuestions} questions.\n\nReturn to dashboard?", 
                "Test Submitted", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Information);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // Raise event to close window and return to dashboard
                TestSubmitted?.Invoke(this, EventArgs.Empty);
            }
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
