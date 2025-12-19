using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Jahoot.Display.Utilities;

/// <summary>
/// Converts bool to Visibility with inverse logic (true = Collapsed, false = Visible).
/// Supports one-way binding only.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Collapsed;
        }
        return false;
    }
}
