using System;
using System.Globalization;
using System.Windows.Data;
using SaludTotal.Models;

namespace SaludTotal.Converters
{
    public class TurnoSeleccionadoToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Si el valor es un turno (no null), retorna true, sino false
            return value is Turno;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
