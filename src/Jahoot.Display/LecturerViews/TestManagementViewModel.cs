using Jahoot.Core.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels; // Added for BaseViewModel
using Jahoot.Display.Utilities; // Added for RelayCommand
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ISubjectService _subjectService;

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
        public ICommand CreateTestCommand { get; }

        public TestManagementViewModel(ITestService testService, ISubjectService subjectService)
        {
            _testService = testService;
            _subjectService = subjectService;

            LoadTestsCommand = new RelayCommand(async _ => await LoadTests());
            ViewTestCommand = new RelayCommand(ViewTest);
            EditTestCommand = new RelayCommand(EditTest);
            DeleteTestCommand = new RelayCommand(DeleteTest);
            CreateTestCommand = new RelayCommand(async _ => await CreateTest());

            LoadTestsCommand.Execute(null);
        }

        private async Task LoadTests()
        {
            Debug.WriteLine("LoadTests method initiated.");
            try
            {
                var tests = await _testService.GetTests();
                if (tests != null)
                {
                    Tests = new ObservableCollection<Test>(tests);
                    Debug.WriteLine($"Successfully loaded {Tests.Count} tests.");
                }
                else
                {
                    Debug.WriteLine("TestService.GetTests returned null.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error in LoadTests: {ex.Message}");
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

        private async Task CreateTest()
        {
            var createTestViewModel = ((App)Application.Current).ServiceProvider.GetService<CreateTestViewModel>();
            if (createTestViewModel != null)
            {
                var createTestWindow = new CreateTestWindow
                {
                    DataContext = createTestViewModel
                };

                createTestWindow.Owner = Application.Current.MainWindow;
                createTestWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                createTestWindow.ShowDialog();

                // Refresh tests after the CreateTestWindow is closed
                await LoadTests();
            }
            else
            {
                MessageBox.Show("Failed to initialise CreateTestViewModel.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
