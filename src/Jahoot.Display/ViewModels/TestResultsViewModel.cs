using Jahoot.Display.Models;
using Jahoot.Display.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace Jahoot.Display.StudentViews
{
    /// <summary>
    /// Shared ViewModel for test results display (used by both TestResultsPage and TestResultsView)
    /// </summary>
    public class TestResultsViewModel : BaseViewModel
    {
        private readonly List<QuestionReviewItem> _reviewItems;
        private int _currentReviewIndex = 0;

        public TestResultSummary ResultSummary { get; }

        private string _performanceMessage = string.Empty;
        public string PerformanceMessage
        {
            get => _performanceMessage;
            set
            {
                _performanceMessage = value;
                OnPropertyChanged();
            }
        }

        private string _reviewQuestionNumber = string.Empty;
        public string ReviewQuestionNumber
        {
            get => _reviewQuestionNumber;
            set
            {
                _reviewQuestionNumber = value;
                OnPropertyChanged();
            }
        }

        private string _reviewQuestionText = string.Empty;
        public string ReviewQuestionText
        {
            get => _reviewQuestionText;
            set
            {
                _reviewQuestionText = value;
                OnPropertyChanged();
            }
        }

        private List<AnswerOptionReview> _reviewOptions = new();
        public List<AnswerOptionReview> ReviewOptions
        {
            get => _reviewOptions;
            set
            {
                _reviewOptions = value;
                OnPropertyChanged();
            }
        }

        private bool _canGoToPrevious = false;
        public bool CanGoToPrevious
        {
            get => _canGoToPrevious;
            set
            {
                _canGoToPrevious = value;
                OnPropertyChanged();
            }
        }

        private string _nextButtonText = "Next ?";
        public string NextButtonText
        {
            get => _nextButtonText;
            set
            {
                _nextButtonText = value;
                OnPropertyChanged();
            }
        }

        public bool HasReviewItems => _reviewItems.Count > 0;

        public TestResultsViewModel(TestResultSummary resultSummary, List<QuestionReviewItem> reviewItems)
        {
            ResultSummary = resultSummary;
            _reviewItems = reviewItems;

            SetPerformanceMessage();
        }

        private void SetPerformanceMessage()
        {
            PerformanceMessage = ResultSummary.ScorePercentage switch
            {
                >= 90 => "?? Outstanding performance! You've mastered this material!",
                >= 80 => "?? Great job! You have a strong understanding of the subject.",
                >= 70 => "?? Good work! Keep practicing to improve further.",
                >= 60 => "?? You passed! Review the material to strengthen your knowledge.",
                _ => "?? Keep studying! Review the questions below to improve."
            };
        }

        public void LoadReviewQuestion()
        {
            if (_currentReviewIndex < 0 || _currentReviewIndex >= _reviewItems.Count)
                return;

            var reviewItem = _reviewItems[_currentReviewIndex];

            ReviewQuestionNumber = $"Question {reviewItem.QuestionNumber} of {_reviewItems.Count}";
            ReviewQuestionText = reviewItem.QuestionText;
            ReviewOptions = reviewItem.Options;

            CanGoToPrevious = _currentReviewIndex > 0;
            NextButtonText = _currentReviewIndex < _reviewItems.Count - 1 ? "Next ?" : "Finish Review";
        }

        public bool GoToPreviousQuestion()
        {
            if (_currentReviewIndex > 0)
            {
                _currentReviewIndex--;
                LoadReviewQuestion();
                return true;
            }
            return false;
        }

        public bool GoToNextQuestion()
        {
            if (_currentReviewIndex < _reviewItems.Count - 1)
            {
                _currentReviewIndex++;
                LoadReviewQuestion();
                return true;
            }
            return false; // Last question reached
        }

        public void StartReview()
        {
            _currentReviewIndex = 0;
            LoadReviewQuestion();
        }
    }
}
