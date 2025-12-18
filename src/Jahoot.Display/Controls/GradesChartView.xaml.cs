using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    /// <summary>
    /// Control for displaying student grades over time as a line chart.
    /// TODO: Replace hardcoded sample data with dynamic data from ItemsSource
    /// TODO: Implement data point generation from collection
    /// TODO: Add data binding for chart rendering
    /// Tracking Issue: Consider creating a GitHub issue for dynamic chart implementation
    /// </summary>
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
            // TODO: Implement chart data point generation when ItemsSource changes
            // This should:
            // 1. Clear existing chart elements
            // 2. Calculate positions for new data points
            // 3. Generate Polyline points from data
            // 4. Add data point ellipses
            // 5. Update X-axis labels with actual test names
            
            // For now, the chart will continue to show hardcoded data
            // When implementing dynamic rendering, use GradeDataPoint model from ItemsSource
        }

        public GradesChartView()
        {
            InitializeComponent();
            // NOTE: Currently displays hardcoded sample data in XAML
            // This is not production-ready and must be replaced with dynamic rendering
        }
    }
}
