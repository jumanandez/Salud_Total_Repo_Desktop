using System;
using System.Globalization;
using System.Windows.Data;

namespace SaludTotal.Desktop.Converters
{
    public class BooleanToSiNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "Sí" : "No";
            if (value is string s)
            {
                if (bool.TryParse(s, out bool result))
                    return result ? "Sí" : "No";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
                return s.Equals("Sí", StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}
