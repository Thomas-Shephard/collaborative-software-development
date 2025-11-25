using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Jahoot.Display.Controls
{
    public partial class SubjectPerformanceView : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SubjectPerformanceView), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public SubjectPerformanceView()
        {
            InitializeComponent();
        }
    }
}
