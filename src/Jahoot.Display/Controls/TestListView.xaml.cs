using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jahoot.Display.Controls
{
    public partial class TestListView : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(TestListView), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(TestListView), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(TestListView), new PropertyMetadata(null));

        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register(nameof(ItemClickCommand), typeof(ICommand), typeof(TestListView), new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ICommand ItemClickCommand
        {
            get => (ICommand)GetValue(ItemClickCommandProperty);
            set => SetValue(ItemClickCommandProperty, value);
        }

        public TestListView()
        {
            InitializeComponent();
            
            // Add event handler for mouse clicks
            this.PreviewMouseLeftButtonDown += TestListView_PreviewMouseLeftButtonDown;
        }

        private void TestListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Find the data context of the clicked element
            var element = e.OriginalSource as DependencyObject;
            
            while (element != null)
            {
                if (element is FrameworkElement fe && fe.DataContext != null && fe.DataContext != this.DataContext)
                {
                    // Check if this is a test item (not the control itself)
                    var dataContext = fe.DataContext;
                    
                    // Execute the ItemClickCommand with the clicked item
                    if (ItemClickCommand?.CanExecute(dataContext) == true)
                    {
                        ItemClickCommand.Execute(dataContext);
                        e.Handled = true;
                        return;
                    }
                }
                
                element = VisualTreeHelper.GetParent(element);
            }
        }
    }
}
