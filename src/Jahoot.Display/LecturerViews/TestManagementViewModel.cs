using Jahoot.Core.Models;
using Jahoot.Display.Services;
using Jahoot.Display.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Jahoot.Display.Commands;
using Jahoot.Display.Models; // Import the new TestDisplayModel

namespace Jahoot.Display.LecturerViews
{
    public class TestManagementViewModel : BaseViewModel
    {
        private readonly ITestService _testService;
        private readonly ISubjectService _subjectService;
        private readonly Func<Test, EditTestViewModel> _editTestViewModelFactory;
        private readonly Func<CreateTestViewModel> _createTestViewModelFactory;

        private ObservableCollection<TestDisplayModel> _tests = new ObservableCollection<TestDisplayModel>();
        public ObservableCollection<TestDisplayModel> Tests
        {
            get => _tests;
            set
            {
                _tests = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Subject> _allSubjects = new ObservableCollection<Subject>();

        public ICommand LoadTestsCommand { get; }
        public ICommand EditTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand CreateTestCommand { get; }

        public TestManagementViewModel(ITestService testService, ISubjectService subjectService, Func<Test, EditTestViewModel> editTestViewModelFactory, Func<CreateTestViewModel> createTestViewModelFactory)
        {
            _testService = testService;
            _subjectService = subjectService;
            _editTestViewModelFactory = editTestViewModelFactory;
            _createTestViewModelFactory = createTestViewModelFactory;

            LoadTestsCommand = new RelayCommand(async _ => await LoadTests());
            EditTestCommand = new AsyncRelayCommand(EditTest);
            DeleteTestCommand = new AsyncRelayCommand(DeleteTest);
            CreateTestCommand = new AsyncRelayCommand(async _ => await CreateTest());
        }

        public async Task InitialiseAsync()
        {
            await LoadAllSubjects();
            await LoadTests();
        }

        private async Task LoadAllSubjects()
        {
            try
            {
                var subjects = await _subjectService.GetSubjects();
                _allSubjects = new ObservableCollection<Subject>(subjects);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading subjects: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTests()
        {
            try
            {
                var tests = await _testService.GetTests();
                if (tests != null)
                {
                    var displayTests = new ObservableCollection<TestDisplayModel>();
                    foreach (var test in tests)
                    {
                        var subject = _allSubjects.FirstOrDefault(s => s.SubjectId == test.SubjectId);
                        displayTests.Add(new TestDisplayModel
                        {
                            TestId = test.TestId,
                            Name = test.Name,
                            NumberOfQuestions = test.NumberOfQuestions,
                            SubjectName = subject?.Name ?? "Unknown Subject"
                        });
                    }
                    Tests = displayTests;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditTest(object? obj)
        {
            if (obj is TestDisplayModel testToEdit) // Changed to TestDisplayModel
            {
                try
                {
                    // Need to get the full Test object from service for editing if EditTestViewModel expects it
                    var fullTest = await _testService.GetTestById(testToEdit.TestId);
                    if (fullTest == null)
                    {
                        MessageBox.Show("Could not find test details for editing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editTestViewModel = _editTestViewModelFactory(fullTest);
                    var editTestWindow = new EditTestWindow(editTestViewModel);

                    editTestWindow.Owner = Application.Current.MainWindow;
                    editTestWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    editTestWindow.ShowDialog();

                    await LoadTests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while editing the test: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DeleteTest(object? obj)
        {
            if (obj is TestDisplayModel testToDelete) // Changed to TestDisplayModel
            {
                try
                {
                    bool hasAttempts = await _testService.HasAttempts(testToDelete.TestId);
                    string message;
                    MessageBoxImage icon;

                    if (hasAttempts)
                    {
                        message = $"WARNING: Students have already attempted '{testToDelete.Name}'. Deleting this test will remove all associated student results. Are you absolutely sure you want to proceed?";
                        icon = MessageBoxImage.Stop;
                    }
                    else
                    {
                        message = $"Are you sure you want to delete '{testToDelete.Name}'?";
                        icon = MessageBoxImage.Warning;
                    }

                    var result = MessageBox.Show(message, "Confirm Deletion", MessageBoxButton.YesNo, icon);
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
            var createTestViewModel = _createTestViewModelFactory();
            var createTestWindow = new CreateTestWindow(createTestViewModel);

            createTestWindow.Owner = Application.Current.MainWindow;
            createTestWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            createTestWindow.ShowDialog();

            await LoadTests();
        }
    }
}
