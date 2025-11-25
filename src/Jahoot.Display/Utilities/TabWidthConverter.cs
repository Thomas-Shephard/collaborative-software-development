using System;
using System.Globalization;
using System.Windows.Data;

namespace Jahoot.Display.Utilities
{
    public class TabWidthConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth && parameter != null && int.TryParse(parameter.ToString(), out int tabCount) && tabCount > 0)
            {
                double adjustedWidth = totalWidth - 10; // Subtract small margin to prevent wrapping
                return adjustedWidth / tabCount;
            }
            return 100;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
