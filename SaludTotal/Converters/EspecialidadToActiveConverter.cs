using System;
using System.Globalization;
using System.Windows.Data;

namespace SaludTotal.Converters
{
    public class EspecialidadToActiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                Console.WriteLine($"EspecialidadToActiveConverter: NULO - value={value}, parameter={parameter}");
                return false;
            }

            string? especialidadSeleccionada = value.ToString();
            string? especialidadBoton = parameter.ToString();

            if (especialidadSeleccionada == null || especialidadBoton == null)
            {
                Console.WriteLine($"EspecialidadToActiveConverter: STRING NULO - especialidadSeleccionada={especialidadSeleccionada}, especialidadBoton={especialidadBoton}");
                return false;
            }

            bool resultado = especialidadSeleccionada.Equals(especialidadBoton, StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"EspecialidadToActiveConverter: COMPARACIÓN '{especialidadSeleccionada}' == '{especialidadBoton}' = {resultado}");
            
            return resultado;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
