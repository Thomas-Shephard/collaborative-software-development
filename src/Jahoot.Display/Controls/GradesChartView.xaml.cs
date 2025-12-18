using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    public partial class GradesChartView : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(GradesChartView),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO: Link chart data rendering to backend data
        }

        public GradesChartView()
        {
            InitializeComponent();
        }
    }
}
