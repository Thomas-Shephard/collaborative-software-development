using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Jahoot.Display.Models;

namespace Jahoot.Display.StudentViews
{
    public partial class TestResultsView : UserControl
    {
        private readonly TestResultSummary _resultSummary;
        private readonly List<QuestionReviewItem> _reviewItems;
        private int _currentReviewIndex = 0;

        public event EventHandler? ReturnToDashboardRequested;

        public TestResultsView(TestResultSummary resultSummary, List<QuestionReviewItem> reviewItems)
        {
            InitializeComponent();

            _resultSummary = resultSummary;
            _reviewItems = reviewItems;

            DataContext = _resultSummary;

            SetPerformanceMessage();
        }

        private void SetPerformanceMessage()
        {
            string message = _resultSummary.ScorePercentage switch
            {
                >= 90 => "?? Outstanding performance! You've mastered this material!",
                >= 80 => "?? Great job! You have a strong understanding of the subject.",
                >= 70 => "?? Good work! Keep practicing to improve further.",
                >= 60 => "?? You passed! Review the material to strengthen your knowledge.",
                _ => "?? Keep studying! Review the questions below to improve."
            };

            PerformanceMessage.Text = message;
        }

        private void ReturnToDashboard_Click(object sender, RoutedEventArgs e)
        {
            ReturnToDashboardRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ReviewAnswers_Click(object sender, RoutedEventArgs e)
        {
            SummaryView.Visibility = Visibility.Collapsed;
            ReviewView.Visibility = Visibility.Visible;

            _currentReviewIndex = 0;
            LoadReviewQuestion();
        }

        private void BackToSummary_Click(object sender, RoutedEventArgs e)
        {
            ReviewView.Visibility = Visibility.Collapsed;
            SummaryView.Visibility = Visibility.Visible;
        }

        private void LoadReviewQuestion()
        {
            if (_currentReviewIndex < 0 || _currentReviewIndex >= _reviewItems.Count)
                return;

            var reviewItem = _reviewItems[_currentReviewIndex];

            // Update header
            ReviewQuestionNumber.Text = $"Question {reviewItem.QuestionNumber} of {_reviewItems.Count}";

            // Hide result indicator since we don't have correct answer data
            ReviewResultIcon.Visibility = Visibility.Collapsed;
            ReviewResultText.Visibility = Visibility.Collapsed;

            // Update question text
            ReviewQuestionText.Text = reviewItem.QuestionText;

            // Update options
            ReviewOptionsControl.ItemsSource = reviewItem.Options;

            // Update navigation buttons
            PrevQuestionButton.IsEnabled = _currentReviewIndex > 0;
            NextQuestionButton.Content = _currentReviewIndex < _reviewItems.Count - 1 ? "Next ?" : "Finish Review";
        }

        private void PreviousQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_currentReviewIndex > 0)
            {
                _currentReviewIndex--;
                LoadReviewQuestion();
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_currentReviewIndex < _reviewItems.Count - 1)
            {
                _currentReviewIndex++;
                LoadReviewQuestion();
            }
            else
            {
                // Last question, go back to summary
                BackToSummary_Click(sender, e);
            }
        }
    }
}
