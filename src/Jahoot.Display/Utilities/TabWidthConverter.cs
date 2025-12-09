using System.Globalization;
using System.Windows.Data;

namespace Jahoot.Display.Utilities
{
    public class TabWidthConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double totalWidth && values[1] != null && int.TryParse(values[1].ToString(), out int tabCount) && tabCount > 0)
            {
                double adjustedWidth = totalWidth - 10; // Subtract small margin to prevent wrapping
                return adjustedWidth / tabCount;
            }
            return 100;
        }

        public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
