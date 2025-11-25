using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    public partial class DashboardHeader : UserControl
    {
        public static readonly DependencyProperty AvailableRolesProperty =
            DependencyProperty.Register("AvailableRoles", typeof(ObservableCollection<string>), typeof(DashboardHeader), new FrameworkPropertyMetadata(new ObservableCollection<string>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ObservableCollection<string> AvailableRoles
        {
            get { return (ObservableCollection<string>)GetValue(AvailableRolesProperty); }
            set { SetValue(AvailableRolesProperty, value); }
        }

        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register("SelectedRole", typeof(string), typeof(DashboardHeader), new FrameworkPropertyMetadata("Lecturer", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SelectedRole
        {
            get { return (string)GetValue(SelectedRoleProperty); }
            set { SetValue(SelectedRoleProperty, value); }
        }

        public static readonly DependencyProperty UserInitialsProperty =
            DependencyProperty.Register("UserInitials", typeof(string), typeof(DashboardHeader), new PropertyMetadata("JD"));

        public string UserInitials
        {
            get { return (string)GetValue(UserInitialsProperty); }
            set { SetValue(UserInitialsProperty, value); }
        }

        public DashboardHeader()
        {
            InitializeComponent();
        }
    }
}
