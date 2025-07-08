using System;
using System.Globalization;
using System.Windows.Data;

namespace SaludTotal.Desktop.Converters
{
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            
            string? fechaHoraString = value.ToString();
            if (string.IsNullOrEmpty(fechaHoraString)) return string.Empty;
            
            // Si contiene espacio, extraer solo la parte de fecha
            if (fechaHoraString.Contains(" "))
            {
                string fechaParte = fechaHoraString.Split(' ')[0];
                return FormatearFecha(fechaParte);
            }
            
            return FormatearFecha(fechaHoraString);
        }

        private string FormatearFecha(string fechaString)
        {
            // Si ya está en formato yyyy-MM-dd, devolverlo directamente
            if (System.Text.RegularExpressions.Regex.IsMatch(fechaString, @"^\d{4}-\d{2}-\d{2}$"))
            {
                return fechaString;
            }
            
            // Si está en formato dd/MM/yyyy
            if (System.Text.RegularExpressions.Regex.IsMatch(fechaString, @"^\d{1,2}/\d{1,2}/\d{4}$"))
            {
                var partes = fechaString.Split('/');
                if (partes.Length == 3)
                {
                    string dia = partes[0].PadLeft(2, '0');
                    string mes = partes[1].PadLeft(2, '0');
                    string año = partes[2];
                    return $"{año}-{mes}-{dia}";
                }
            }
            
            // Si está en formato dd-MM-yyyy
            if (System.Text.RegularExpressions.Regex.IsMatch(fechaString, @"^\d{1,2}-\d{1,2}-\d{4}$"))
            {
                var partes = fechaString.Split('-');
                if (partes.Length == 3)
                {
                    string dia = partes[0].PadLeft(2, '0');
                    string mes = partes[1].PadLeft(2, '0');
                    string año = partes[2];
                    return $"{año}-{mes}-{dia}";
                }
            }
            
            // Para fechas en formato ISO con zona horaria (2024-01-17T10:30:00Z)
            if (fechaString.Contains("T"))
            {
                string fechaParte = fechaString.Split('T')[0];
                if (System.Text.RegularExpressions.Regex.IsMatch(fechaParte, @"^\d{4}-\d{2}-\d{2}$"))
                {
                    return fechaParte;
                }
            }
            
            // Como último recurso, intentar parsear pero evitando conversiones de zona horaria
            try
            {
                // Usar ParseExact con formatos específicos para evitar ambigüedades
                string[] formatos = {
                    "yyyy-MM-dd",
                    "dd/MM/yyyy", 
                    "MM/dd/yyyy",
                    "dd-MM-yyyy",
                    "MM-dd-yyyy",
                    "yyyy/MM/dd"
                };
                
                foreach (string formato in formatos)
                {
                    if (DateTime.TryParseExact(fechaString, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                    {
                        return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                }
            }
            catch
            {
                // Si todo falla, devolver el string original
            }
            
            return fechaString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
