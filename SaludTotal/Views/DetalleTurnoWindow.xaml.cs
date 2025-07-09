using System;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Models;
using SaludTotal.Services;
using SaludTotal.Desktop.Converters;
using System.Globalization;

namespace SaludTotal.Desktop.Views
{
    public partial class DetalleTurnoWindow : Window
    {
        private Turno _turno;
        public bool Confirmado { get; private set; } = false;

        // Constructor nuevo para la gestión de turnos existentes
        public DetalleTurnoWindow(Turno turno)
        {
            InitializeComponent();
            _turno = turno;
            CargarDatosTurno();
        }

        // Constructor compatible con NuevoTurnoWindow (mantiene compatibilidad)
        public DetalleTurnoWindow(Paciente paciente, string especialidad, string doctor, string fecha, string hora)
        {
            InitializeComponent();
            
            // Crear un turno temporal para mostrar la información
            _turno = new Turno
            {
                Paciente = paciente,
                Profesional = new Profesional 
                { 
                    NombreApellido = doctor,
                    Especialidad = new Especialidad { Nombre = especialidad }
                },
                Fecha = fecha,
                Hora = hora
            };
            
            CargarDatosTurno();
        }

        private Especialidad TryParseEspecialidad(string especialidad)
        {
            return new Especialidad { Nombre = especialidad };
        }

        private void CargarDatosTurno()
        {
            if (_turno != null)
            {
                // Debug: Imprimir información del turno
                System.Diagnostics.Debug.WriteLine($"DEBUG: Cargando turno ID: {_turno.Id}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Profesional: {_turno.Profesional?.NombreCompleto ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: Especialidad: {_turno.Profesional?.Especialidad?.Nombre ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"DEBUG: EspecialidadId: {_turno.Profesional?.EspecialidadId}");

                // Información del Paciente
                PacienteNombre.Text = _turno.Paciente?.NombreCompleto ?? "No disponible";
                PacienteTelefono.Text = _turno.Paciente?.Telefono ?? "No disponible";
                PacienteEmail.Text = _turno.Paciente?.Email ?? "No disponible";

                // Información del Doctor
                DoctorNombre.Text = _turno.Profesional?.NombreCompleto ?? "No disponible";
                
                // Intentar obtener la especialidad de diferentes maneras
                string especialidad = "No disponible";
                if (_turno.Profesional?.Especialidad?.Nombre != null)
                {
                    especialidad = _turno.Profesional.Especialidad.Nombre;
                }
                else if (_turno.Profesional?.EspecialidadId > 0)
                {
                    // Si tenemos ID pero no nombre, mapear según los IDs conocidos
                    especialidad = _turno.Profesional.EspecialidadId switch
                    {
                        1 => "Cardiología",
                        2 => "Ginecología", 
                        3 => "Pediatría",
                        4 => "Clínica General",
                        _ => $"Especialidad ID: {_turno.Profesional.EspecialidadId}"
                    };
                }
                
                DoctorEspecialidad.Text = especialidad;
                System.Diagnostics.Debug.WriteLine($"DEBUG: Especialidad final mostrada: {especialidad}");

                // Fecha original - usar el convertidor para formatear
                var converter = new DateTimeFormatConverter();
                string fechaFormateada = (converter.Convert(_turno.Fecha, typeof(string), null!, CultureInfo.CurrentCulture) as string) ?? _turno.Fecha;
                FechaOriginal.Text = $"{fechaFormateada} {_turno.Hora}";
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void CancelarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null || _turno.Id <= 0)
            {
                MessageBox.Show("No se puede aceptar un turno no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var apiService = new Services.ApiService();
            try
            {
                ResultadoApi result = await apiService.CancelarTurnoAsync(_turno.Id);
                if (result.Success)
                {
                    MessageBox.Show(result.Mensaje, "Cancelar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
                    Confirmado = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(("Mensaje: " + result.Mensaje + "\n" + "Detalle: " + result.Detalle), "Aceptar Turno", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aceptar turno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReprogramarTurno_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar lógica para reprogramar turno
            string nuevaFecha = CalendarFecha.SelectedDate?.ToShortDateString() ?? "No seleccionada";
            string nuevaHora = (ComboHora.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "No seleccionada";
            
            MessageBox.Show($"Reprogramar turno:\nFecha: {nuevaFecha}\nHora: {nuevaHora}\n\nFuncionalidad por implementar", 
                           "Reprogramar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void AceptarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null || _turno.Id <= 0)
            {
                MessageBox.Show("No se puede aceptar un turno no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var apiService = new SaludTotal.Desktop.Services.ApiService();
            try
            {
                ResultadoApi result = await apiService.AceptarTurnoAsync(_turno.Id);
                if (result.Success)
                {
                    MessageBox.Show(result.Mensaje, "Aceptar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
                    Confirmado = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(("Mensaje: " + result.Mensaje + "\n" + "Detalle: " + result.Detalle), "Aceptar Turno", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aceptar turno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RechazarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null || _turno.Id <= 0)
            {
                MessageBox.Show("No se puede rechazar un turno no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var apiService = new SaludTotal.Desktop.Services.ApiService();
            try
            {
                ResultadoApi result = await apiService.RechazarTurnoAsync(_turno.Id);
                if (result.Success)
                {
                    MessageBox.Show(result.Mensaje, "Rechazar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
                    Confirmado = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(("Mensaje: " + result.Mensaje + "\n" + "Detalle: " + result.Detalle), "Rechazar Turno", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al rechazar turno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Métodos para compatibilidad con NuevoTurnoWindow
        private void Enviar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.DialogResult = false;
            this.Close();
        }

        // Evento cuando se selecciona una fecha en el calendar
        private void CalendarFecha_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarFechaNueva();
        }

        // Evento cuando se selecciona una hora en el combobox
        private void ComboHora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarFechaNueva();
        }

        // Método para actualizar la fecha nueva combinando fecha y hora
        private void ActualizarFechaNueva()
        {
            string fechaParte = "--";
            string horaParte = "--";

            // Obtener fecha seleccionada
            if (CalendarFecha.SelectedDate.HasValue)
            {
                var converter = new DateTimeFormatConverter();
                fechaParte = (converter.Convert(CalendarFecha.SelectedDate.Value.ToString("yyyy-MM-dd"), typeof(string), null!, CultureInfo.CurrentCulture) as string) ?? CalendarFecha.SelectedDate.Value.ToString("yyyy-MM-dd");
            }

            // Obtener hora seleccionada
            if (ComboHora.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                horaParte = item.Content.ToString() ?? "--";
            }

            // Actualizar los TextBlocks
            if (fechaParte != "--" && horaParte != "--")
            {
                FechaNueva.Text = fechaParte;
                HoraNueva.Text = horaParte;
            }
            else if (fechaParte != "--")
            {
                FechaNueva.Text = fechaParte;
                HoraNueva.Text = "--";
            }
            else
            {
                FechaNueva.Text = "--";
                HoraNueva.Text = "--";
            }
        }
    }
}
