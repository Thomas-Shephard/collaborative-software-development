using Jahoot.Core.Models;
using Jahoot.Display.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Jahoot.Display.LecturerViews
{
    public class TestManagementViewModel : BaseViewModel
    {
        private readonly ITestService _testService;

        private ObservableCollection<Test> _tests = new ObservableCollection<Test>();
        public ObservableCollection<Test> Tests
        {
            get => _tests;
            set
            {
                _tests = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadTestsCommand { get; }
        public ICommand ViewTestCommand { get; }
        public ICommand EditTestCommand { get; }
        public ICommand DeleteTestCommand { get; }

        public TestManagementViewModel(ITestService testService)
        {
            _testService = testService;

            LoadTestsCommand = new RelayCommand(async _ => await LoadTests());
            ViewTestCommand = new RelayCommand(ViewTest);
            EditTestCommand = new RelayCommand(EditTest);
            DeleteTestCommand = new RelayCommand(DeleteTest);

            LoadTestsCommand.Execute(null);
        }

        private async Task LoadTests()
        {
            try
            {
                var tests = await _testService.GetTests();
                Tests = new ObservableCollection<Test>(tests);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewTest(object? obj)
        {
            // Not implemented yet
        }

        private void EditTest(object? obj)
        {
            // Not implemented yet
        }

        private async void DeleteTest(object? obj)
        {
            if (obj is Test testToDelete)
            {
                try
                {
                    var result = MessageBox.Show($"Are you sure you want to delete {testToDelete.Name}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        await _testService.DeleteTest(testToDelete.TestId);
                        await LoadTests();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
