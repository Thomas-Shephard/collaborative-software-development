using System.Collections.ObjectModel;
using System.Windows.Controls;
using Jahoot.Display.Models;

namespace Jahoot.Display.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        public string AdminInitials
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "AD";

        public ObservableCollection<TabItem> TabItems { get; set; }

        public ObservableCollection<string> AvailableRoles
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = new ObservableCollection<string> { "Student", "Lecturer", "Admin" };

        public string SelectedRole
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Admin";

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
            }
        }

        public AdminDashboardViewModel()
        {
            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Manage Subjects" },
                new TabItem { Header = "Manage Lecturers" }
            };
        }
    }
}