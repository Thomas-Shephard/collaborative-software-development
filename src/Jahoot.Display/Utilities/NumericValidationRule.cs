using System.Globalization;
using System.Windows.Controls;

namespace Jahoot.Display.Utilities
{
    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string stringValue && int.TryParse(stringValue, out int number))
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Please enter a valid number.");
        }
    }
}