using System;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Models;
using SaludTotal.Services;
using SaludTotal.Desktop.Converters;
using System.Globalization;
using System.Linq;
using System.Text;
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
            _ = CargarHorariosLaboralesReprogramacionAsync();
            this.DataContext = _turno;
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

        private async void ReprogramarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null || _turno.Id <= 0)
            {
                MessageBox.Show("No se puede reprogramar un turno no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Obtener doctorId, fecha y hora
            int doctorId = _turno.Profesional?.DoctorId ?? _turno.DoctorId;
            string nuevaFecha = CalendarFecha.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            string nuevaHora = (ComboHora.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? string.Empty;

            if (doctorId <= 0 || string.IsNullOrWhiteSpace(nuevaFecha) || string.IsNullOrWhiteSpace(nuevaHora))
            {
                MessageBox.Show("Debe seleccionar un doctor, fecha y hora válidos para reprogramar el turno.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }else if(nuevaFecha == _turno.Fecha && nuevaHora == _turno.Hora)
            {
                MessageBox.Show("La fecha y hora seleccionadas son las mismas que las originales. Por favor, elija una fecha y hora diferentes.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var apiService = new SaludTotal.Desktop.Services.ApiService();
            try
            {
                ResultadoApi result = await apiService.ReprogramarTurnoAsync(_turno.Id, doctorId, nuevaFecha, nuevaHora);
                if (result.Success)
                {
                    string text = result.Mensaje + "\n" +
                                  $"Nuevo turno: {nuevaFecha} {nuevaHora}\n" +
                                  $"Doctor: {_turno.Profesional?.NombreCompleto ?? "No disponible"}";

                    MessageBox.Show(text, "Reprogramar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
                    Confirmado = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else if(result.Errores != null)
                {
                    // Mostrar errores específicos si existen
                    var sb = new StringBuilder();
                    sb.AppendLine(result.Mensaje);

                    foreach (var campo in result.Errores)
                    {
                        foreach (var mensaje in campo.Value)
                        {
                            sb.AppendLine($"- {mensaje}");
                        }
                    }
                    MessageBox.Show(sb.ToString(), "Errores", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(("Mensaje: " + result.Mensaje + "\n" + "Detalle: " + result.Detalle), "Reprogramar Turno", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reprogramar turno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AceptarTurno_Click(object sender, RoutedEventArgs e)
        {
               
            if (_turno == null || _turno.Id <= 0)
            {
                MessageBox.Show("No se puede aceptar un turno no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AceptarTurnoButton.IsEnabled = false;
            AceptarTurnoButton.Content = "Procesando...";
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
                else if(result.Errores != null)
                {
                    // Mostrar errores específicos si existen
                    var sb = new StringBuilder();
                    sb.AppendLine(result.Mensaje);

                    foreach (var campo in result.Errores)
                    {
                        foreach (var mensaje in campo.Value)
                        {
                            sb.AppendLine($"- {mensaje}");
                        }
                    }
                    MessageBox.Show(sb.ToString(), "Errores", MessageBoxButton.OK, MessageBoxImage.Error);
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
            finally
            {

                AceptarTurnoButton.Content = "Aceptar";
                AceptarTurnoButton.IsEnabled = true;
            }
        }

        private async void RechazarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null || _turno.Id <= 0)
            {
                MessageBox.Show("No se puede rechazar un turno no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var inputBox = new InputBoxWindow("Mensaje de rechazo", "Ingrese el mensaje que se enviará al paciente (opcional):");
            inputBox.Owner = this;
            bool? result = inputBox.ShowDialog();
            if (result != true) return;

            string? mensaje = inputBox.UserInput;

            var apiService = new SaludTotal.Desktop.Services.ApiService();
            try
            {
                ResultadoApi apiResult = await apiService.RechazarTurnoAsync(_turno.Id, mensaje);
                if (apiResult.Success)
                {
                    MessageBox.Show(apiResult.Mensaje, "Rechazar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
                    Confirmado = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(("Mensaje: " + apiResult.Mensaje + "\n" + "Detalle: " + apiResult.Detalle), "Rechazar Turno", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void AceptarCancelacion_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null) return;
            
            var _apiService = new SaludTotal.Desktop.Services.ApiService();
            var resultado = await _apiService.AceptarSolicitudCancelacionAsync(_turno.Id);
            if (resultado.Errores != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(resultado.Mensaje);
                foreach (var campo in resultado.Errores)
                {
                    foreach (var mensaje in campo.Value)
                    {
                        sb.AppendLine($"- {mensaje}");
                    }
                }
                MessageBox.Show(sb.ToString(), "Errores", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(resultado.Mensaje, "Resultado", MessageBoxButton.OK, MessageBoxImage.Information);
            // Aquí puedes actualizar la UI o cerrar la ventana si lo deseas
            this.Close();
        }

        private async void RechazarCancelacion_Click(object sender, RoutedEventArgs e)
        {
            if (_turno == null) return;

            
            var _apiService = new SaludTotal.Desktop.Services.ApiService();
            var resultado = await _apiService.RechazarSolicitudCancelacionAsync(_turno.Id);
            if (resultado.Errores != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(resultado.Mensaje);
                foreach (var campo in resultado.Errores)
                {
                    foreach (var mensaje in campo.Value)
                    {
                        sb.AppendLine($"- {mensaje}");
                    }
                }
                MessageBox.Show(sb.ToString(), "Errores", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(resultado.Mensaje, "Resultado", MessageBoxButton.OK, MessageBoxImage.Information);
            // Aquí puedes actualizar la UI o cerrar la ventana si lo deseas
            this.Close();
        }
        // Evento cuando se selecciona una fecha en el calendar
        private async void CalendarFecha_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarFechaNueva();
            // --- NUEVO: Cargar horarios disponibles para la reprogramación ---
            if (_turno?.Profesional != null && _turno.Profesional.DoctorId > 0 && CalendarFecha.SelectedDate.HasValue)
            {
                try
                {
                    var apiService = new Services.ApiService();
                    var fecha = CalendarFecha.SelectedDate.Value.ToString("yyyy-MM-dd");
                    var slots = await apiService.GetSlotsTurnosDisponiblesAsync(_turno.Profesional.DoctorId, fecha);
                    ComboHora.Items.Clear();
                    if (slots != null && slots.Any())
                    {
                        foreach (var slot in slots)
                        {
                            ComboBoxItem item = new ComboBoxItem { Content = slot.Hora, Tag = slot.Hora };
                            ComboHora.Items.Add(item);
                        }
                        ComboHora.SelectedIndex = 0;
                    }
                    else
                    {
                        ComboHora.Items.Add(new ComboBoxItem { Content = "No hay horarios disponibles", IsEnabled = false });
                        ComboHora.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    ComboHora.Items.Clear();
                    ComboHora.Items.Add(new ComboBoxItem { Content = "Error al cargar horarios", IsEnabled = false });
                    ComboHora.SelectedIndex = 0;
                }
            }
        }

        // Evento cuando se selecciona una hora en el combobox
        private void ComboHora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarFechaNueva();
        }

        // --- NUEVO: Mostrar horarios laborales del doctor en la sección de reprogramación ---
        private async Task CargarHorariosLaboralesReprogramacionAsync()
        {
            if (_turno?.Profesional != null && _turno.Profesional.DoctorId > 0)
            {
                try
                {
                    var apiService = new Services.ApiService();
                    var horarios = await apiService.GetHorariosLaboralesAsync(_turno.Profesional.DoctorId);
                    if (DiasLaboralesTextBlock != null)
                    {
                        if (horarios.Any())
                        {
                            var dias = horarios.Select(h => $"{((DiasSemana)h.DiaSemana).ToString()}: {h.HoraInicio} - {h.HoraFin}");
                            DiasLaboralesTextBlock.Text = "Días laborales: " + string.Join(", ", dias);
                        }
                        else
                        {
                            DiasLaboralesTextBlock.Text = "El doctor no tiene días laborales configurados.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (DiasLaboralesTextBlock != null)
                        DiasLaboralesTextBlock.Text = "Error al cargar días laborales.";
                }
            }
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
