using Jahoot.Core.Models;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    public partial class SubjectListControl : UserControl
    {
        public SubjectListControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SubjectListControl), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SubjectListControl), new PropertyMetadata(null));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SubjectListControl), new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty ShowStatusProperty =
            DependencyProperty.Register("ShowStatus", typeof(bool), typeof(SubjectListControl), new PropertyMetadata(true));

        public bool ShowStatus
        {
            get { return (bool)GetValue(ShowStatusProperty); }
            set { SetValue(ShowStatusProperty, value); }
        }

        public event Func<object, Subject, Task>? EditSubjectRequested;

        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Subject subject)
            {
                if (EditSubjectRequested != null)
                {
                    await EditSubjectRequested.Invoke(this, subject);
                }
            }
        }
    }
}
