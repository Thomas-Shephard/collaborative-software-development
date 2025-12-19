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

namespace Jahoot.Display.LecturerViews
{
    public class TestManagementViewModel : BaseViewModel
    {
        private readonly ITestService _testService;
        private readonly ISubjectService _subjectService;
        private readonly Func<Test, EditTestViewModel> _editTestViewModelFactory;

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
        public ICommand EditTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand CreateTestCommand { get; }

        public TestManagementViewModel(ITestService testService, ISubjectService subjectService, Func<Test, EditTestViewModel> editTestViewModelFactory)
        {
            _testService = testService;
            _subjectService = subjectService;
            _editTestViewModelFactory = editTestViewModelFactory;

            LoadTestsCommand = new RelayCommand(async _ => await LoadTests());
            EditTestCommand = new RelayCommand(EditTest);
            DeleteTestCommand = new RelayCommand(DeleteTest);
            CreateTestCommand = new RelayCommand(async _ => await CreateTest());

            LoadTestsCommand.Execute(null);
        }

        private async Task LoadTests()
        {

            try
            {
                var tests = await _testService.GetTests();
                if (tests != null)
                {
                    Tests = new ObservableCollection<Test>(tests);
    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private async void EditTest(object? obj)
        {
            if (obj is Test testToEdit)
            {
                var editTestViewModel = _editTestViewModelFactory(testToEdit);
                var editTestWindow = new EditTestWindow(editTestViewModel);

                editTestWindow.Owner = Application.Current.MainWindow;
                editTestWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                editTestWindow.ShowDialog();

                // Refresh tests after the EditTestWindow is closed
                await LoadTests();
            }
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
                var createTestWindow = new CreateTestWindow(createTestViewModel);

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
