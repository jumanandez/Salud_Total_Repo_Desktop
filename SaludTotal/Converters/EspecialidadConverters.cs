using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SaludTotal.Converters
{
    public class EspecialidadToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return new SolidColorBrush(Color.FromRgb(232, 244, 253)); // Color por defecto

            string? especialidadSeleccionada = value.ToString();
            string? especialidadBoton = parameter.ToString();

            if (especialidadSeleccionada == especialidadBoton)
            {
                return new SolidColorBrush(Color.FromRgb(74, 144, 226)); // Color activo (azul)
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(232, 244, 253)); // Color inactivo (azul claro)
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EspecialidadToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return new SolidColorBrush(Color.FromRgb(44, 62, 80)); // Color por defecto

            string? especialidadSeleccionada = value.ToString();
            string? especialidadBoton = parameter.ToString();

            if (especialidadSeleccionada == especialidadBoton)
            {
                return new SolidColorBrush(Colors.White); // Texto blanco cuando está activo
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(44, 62, 80)); // Texto oscuro cuando está inactivo
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
