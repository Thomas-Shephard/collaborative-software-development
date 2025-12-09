using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    public partial class NavigationalTabs : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(NavigationalTabs), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(NavigationalTabs), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationalTabs));

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public static readonly DependencyProperty TabWidthConverterParameterProperty =
            DependencyProperty.Register("TabWidthConverterParameter", typeof(object), typeof(NavigationalTabs), new PropertyMetadata(null));

        public object TabWidthConverterParameter
        {
            get { return GetValue(TabWidthConverterParameterProperty); }
            set { SetValue(TabWidthConverterParameterProperty, value); }
        }

        public NavigationalTabs()
        {
            InitializeComponent();
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedIndex = MainTabs.SelectedIndex;
            RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, sender));
        }
    }
}
