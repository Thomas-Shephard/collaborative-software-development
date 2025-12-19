using System.Windows;
using System.Windows.Controls;
using Jahoot.Display.Models;
using Jahoot.Display.ViewModels;

namespace Jahoot.Display.StudentViews
{
    public partial class TestResultsView : UserControl
    {
        private readonly TestResultsViewModel _viewModel;

        public event EventHandler? ReturnToDashboardRequested;

        public TestResultsView(TestResultSummary resultSummary, List<QuestionReviewItem> reviewItems)
        {
            InitializeComponent();

            _viewModel = new TestResultsViewModel(resultSummary, reviewItems);
            DataContext = _viewModel;

            PerformanceMessage.Text = _viewModel.PerformanceMessage;
        }

        private void ReturnToDashboard_Click(object sender, RoutedEventArgs e)
        {
            ReturnToDashboardRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ReviewAnswers_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.HasReviewItems)
                return;

            SummaryView.Visibility = Visibility.Collapsed;
            ReviewView.Visibility = Visibility.Visible;

            _viewModel.StartReview();
            UpdateReviewUI();
        }

        private void BackToSummary_Click(object sender, RoutedEventArgs e)
        {
            ReviewView.Visibility = Visibility.Collapsed;
            SummaryView.Visibility = Visibility.Visible;
        }

        private void UpdateReviewUI()
        {
            ReviewQuestionNumber.Text = _viewModel.ReviewQuestionNumber;
            ReviewQuestionText.Text = _viewModel.ReviewQuestionText;
            ReviewOptionsControl.ItemsSource = _viewModel.ReviewOptions;
            PrevQuestionButton.IsEnabled = _viewModel.CanGoToPrevious;
            NextQuestionButton.Content = _viewModel.NextButtonText;
        }

        private void PreviousQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.GoToPreviousQuestion())
            {
                UpdateReviewUI();
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.GoToNextQuestion())
            {
                // Last question reached, go back to summary
                BackToSummary_Click(sender, e);
            }
            else
            {
                UpdateReviewUI();
            }
        }
    }
}
