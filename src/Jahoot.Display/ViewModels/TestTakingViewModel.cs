using System.Collections.ObjectModel;
using System.Windows.Input;
using Jahoot.Core.Models;
using Jahoot.Display.Commands;
using Jahoot.Display.Services;
using System.Diagnostics;
using System.Windows;
using System.Net.Http;
using Jahoot.Display.Models;
using Jahoot.Display.StudentViews;

namespace Jahoot.Display.ViewModels
{
    public class TestTakingViewModel : BaseViewModel
    {
        private readonly ITestService? _testService;

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
                OnPropertyChanged(nameof(IsLastQuestion));
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
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
        private readonly Dictionary<int, int?> _selectedAnswers = new();
        private readonly DateTime _testStartTime;
        private int _currentTestId;

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SubmitCommand { get; }

        // Event to notify when test is submitted and window should close
        public event EventHandler? TestSubmitted;
        public event EventHandler<(TestResultSummary Summary, List<QuestionReviewItem> ReviewItems)>? ShowResults;

        public TestTakingViewModel() : this(null)
        {
        }

        public TestTakingViewModel(ITestService? testService)
        {
            _testService = testService;
            _testStartTime = DateTime.Now;
            NextCommand = new RelayCommand<object>(() => GoToNext(), () => CanGoNext);
            BackCommand = new RelayCommand<object>(() => GoToBack(), () => CanGoBack);
            SubmitCommand = new RelayCommand<object>(
                async () => await SubmitTest(),
                () => CanSubmit
            );
        }

        /// <summary>
        /// Loads a test from the backend API
        /// </summary>
        public async Task LoadTestAsync(int testId, string testName, string subjectName)
        {
            if (_testService == null)
            {
                MessageBox.Show("Test service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                IsLoading = true;
                TestName = testName;
                SubjectName = subjectName;
                _currentTestId = testId;
                _selectedAnswers.Clear();
                CurrentQuestionIndex = 0;

                var testDetails = await _testService.GetTestDetailsAsync(testId);
                
                if (testDetails == null || testDetails.Questions == null || !testDetails.Questions.Any())
                {
                    MessageBox.Show($"Unable to load test {testId} - no questions available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Convert API response to Question objects
                _questions = testDetails.Questions.Select(q => new Question
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    Options = q.Options.Select(o => new QuestionOption
                    {
                        QuestionOptionId = o.QuestionOptionId,
                        QuestionId = q.QuestionId,
                        OptionText = o.OptionText,
                        IsCorrect = false // Backend doesn't send correct answers to prevent cheating
                    }).ToList().AsReadOnly(),
                    CreatedAt = default, // Not provided by backend
                    UpdatedAt = default  // Not provided by backend
                }).ToList();

                TotalQuestions = _questions.Count;
                
                if (TotalQuestions == 0)
                {
                    MessageBox.Show($"Test {testId} loaded but has no questions", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                LoadCurrentQuestion();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads a mock test with 5 questions based on the subject category (fallback for testing)
        /// </summary>
        [Obsolete("Use LoadTestAsync instead to load real test data from the API")]
        public void LoadMockTest(int subjectCategoryId, string testName, string subjectName)
        {
            TestName = testName;
            SubjectName = subjectName;
            _selectedAnswers.Clear();
            CurrentQuestionIndex = 0;

            _questions = subjectCategoryId switch
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
                    Text = "What is the derivative of f(x) = 3x� + 2x - 5?",
                    Options = new List<QuestionOption>
                    {
                        new QuestionOption { QuestionOptionId = 1, QuestionId = 1, OptionText = "6x + 2", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 2, QuestionId = 1, OptionText = "3x + 2", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 3, QuestionId = 1, OptionText = "6x + 5", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 4, QuestionId = 1, OptionText = "3x� + 2", IsCorrect = false }
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
                        new QuestionOption { QuestionOptionId = 5, QuestionId = 2, OptionText = "x� + C", IsCorrect = true },
                        new QuestionOption { QuestionOptionId = 6, QuestionId = 2, OptionText = "2x� + C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 7, QuestionId = 2, OptionText = "x + C", IsCorrect = false },
                        new QuestionOption { QuestionOptionId = 8, QuestionId = 2, OptionText = "2x + C", IsCorrect = false }
                    }.AsReadOnly(),
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
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
                    CreatedAt = _testStartTime,
                    UpdatedAt = _testStartTime
                },
                new Question
                {
                    QuestionId = 4,
                    Text = "What is 2�?",
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
                        new QuestionOption { QuestionOptionId = 11, QuestionId = 3, OptionText = "O(n�)", IsCorrect = false },
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
            
            // Deselect all other options
            var otherOptions = CurrentOptions.Where(o => o != selectedOption);
            foreach (var option in otherOptions)
            {
                option.IsSelected = false;
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

        private async Task SubmitTest()
        {
            if (_testService == null)
            {
                Debug.WriteLine("Test service is not available.");
                return;
            }

            try
            {
                IsLoading = true;

                // Convert answers dictionary to match what backend expects (questionId -> optionId)
                var answersToSubmit = new Dictionary<int, int>();
                foreach (var kvp in _selectedAnswers)
                {
                    if (kvp.Value.HasValue)
                    {
                        answersToSubmit[kvp.Key] = kvp.Value.Value;
                    }
                }

                // Submit to backend and get official score
                var submissionResult = await _testService.SubmitTestAsync(_currentTestId, answersToSubmit);

                if (submissionResult == null)
                {
                    Debug.WriteLine("Failed to submit test - null response from server");
                    return;
                }

                var timeTaken = DateTime.Now - _testStartTime;

                // Use backend-calculated score
                // Calculate correct answers from percentage, then derive incorrect to avoid rounding errors
                int correctAnswers = (int)Math.Round(submissionResult.ScorePercentage * TotalQuestions / 100.0);
                
                var resultSummary = new TestResultSummary
                {
                    TestName = submissionResult.TestName,
                    SubjectName = submissionResult.SubjectName,
                    TotalQuestions = TotalQuestions,
                    CorrectAnswers = correctAnswers,
                    IncorrectAnswers = TotalQuestions - correctAnswers, // Ensure sum equals TotalQuestions
                    ScorePercentage = Math.Round(submissionResult.ScorePercentage, 1),
                    Grade = submissionResult.ScorePercentage >= 90 ? "A" :
                           submissionResult.ScorePercentage >= 80 ? "B" :
                           submissionResult.ScorePercentage >= 70 ? "C" :
                           submissionResult.ScorePercentage >= 60 ? "D" : "F",
                    CompletedAt = submissionResult.CompletedDate,
                    TimeTaken = timeTaken
                };

                // Create a review showing questions and what user answered
                // Note: Backend doesn't return correct answer information to prevent cheating,
                // so we can't show which answers were right/wrong. Review only shows selected answers.
                var reviewItems = new List<QuestionReviewItem>();
                for (int i = 0; i < _questions.Count; i++)
                {
                    var question = _questions[i];
                    var selectedOptionId = _selectedAnswers.GetValueOrDefault(question.QuestionId);

                    var options = question.Options.Select(o => new AnswerOptionReview
                    {
                        OptionText = o.OptionText,
                        IsCorrectAnswer = false, // Backend doesn't provide correct answers for security
                        WasSelected = o.QuestionOptionId == selectedOptionId
                    }).ToList();

                    reviewItems.Add(new QuestionReviewItem
                    {
                        QuestionNumber = i + 1,
                        QuestionText = question.Text,
                        Options = options,
                        IsCorrect = false // Can't determine correctness without backend data
                    });
                }

                // Show results inline if handler is attached, otherwise open window
                if (ShowResults != null)
                {
                    ShowResults?.Invoke(this, (resultSummary, reviewItems));
                }
                else
                {
                    var resultsPage = new TestResultsPage(resultSummary, reviewItems);
                    resultsPage.Show();
                }
                
                // Notify that test was successfully submitted and results are displayed
                // This triggers dashboard refresh and is only called on successful submission
                TestSubmitted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error submitting test: {ex.Message}");
                // TestSubmitted is NOT invoked on error - dashboard won't refresh
            }
            finally
            {
                IsLoading = false;
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
}
