using System;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.Services;
using System.Threading.Tasks;

namespace SaludTotal.Desktop.Views
{
    public partial class NuevoTurnoWindow : Window
    {
        private readonly ApiService _apiService;

        public NuevoTurnoWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            
            // Establecer fecha actual por defecto
            FechaCalendar.SelectedDate = DateTime.Today;
            
            // Seleccionar primera opción por defecto en ComboBoxes
            EspecialidadComboBox.SelectedIndex = 0;
            DoctorComboBox.SelectedIndex = 0;
            HoraComboBox.SelectedIndex = 0;
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            // Navegar de vuelta a la ventana de Administración de Turnos
            var administracionWindow = new AdministracionTurnosWindow();
            administracionWindow.Show();
            this.Close();
        }

        private async void CrearTurno_Click(object sender, RoutedEventArgs e)
        {
            // Validar que todos los campos estén completos
            if (string.IsNullOrWhiteSpace(NombreApellidoTextBox.Text))
            {
                MessageBox.Show("Por favor, ingrese el nombre y apellido del paciente.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                NombreApellidoTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Por favor, ingrese el email del paciente.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }

            // Validar formato del email
            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Por favor, ingrese un email válido.", "Email inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }

            if (EspecialidadComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione una especialidad.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                EspecialidadComboBox.Focus();
                return;
            }

            if (DoctorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un doctor.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                DoctorComboBox.Focus();
                return;
            }

            if (FechaCalendar.SelectedDate == null)
            {
                MessageBox.Show("Por favor, seleccione una fecha.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HoraComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione una hora.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                HoraComboBox.Focus();
                return;
            }

            // Validar que la fecha no sea en el pasado
            if (FechaCalendar.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("No se puede crear un turno para una fecha pasada.", "Fecha inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Deshabilitar el botón para evitar doble envío
                var botonCrear = sender as Button;
                if (botonCrear != null)
                {
                    botonCrear.IsEnabled = false;
                    botonCrear.Content = "Creando...";
                }

                // Mapear especialidades y doctores a IDs (esto debería venir de la API en una implementación real)
                var especialidadId = GetEspecialidadId(((ComboBoxItem)EspecialidadComboBox.SelectedItem).Content.ToString());
                var doctorId = GetDoctorId(((ComboBoxItem)DoctorComboBox.SelectedItem).Content.ToString());
                
                // Crear el objeto de solicitud
                var nuevoTurnoRequest = new NuevoTurnoRequest
                {
                    PacienteNombreApellido = NombreApellidoTextBox.Text.Trim(),
                    PacienteTelefono = string.IsNullOrWhiteSpace(TelefonoTextBox.Text) ? null : TelefonoTextBox.Text.Trim(),
                    PacienteEmail = EmailTextBox.Text.Trim(),
                    DoctorId = doctorId,
                    Fecha = FechaCalendar.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Hora = ((ComboBoxItem)HoraComboBox.SelectedItem).Content.ToString(),
                    EspecialidadId = especialidadId
                };

                // Llamar a la API para crear el turno
                var turnoCreado = await _apiService.CrearTurnoAsync(nuevoTurnoRequest);

                // Mostrar mensaje de éxito
                var mensaje = $"Turno creado exitosamente:\n\n" +
                             $"ID del Turno: {turnoCreado.Id}\n" +
                             $"Paciente: {turnoCreado.Paciente?.NombreCompleto ?? nuevoTurnoRequest.PacienteNombreApellido}\n" +
                             $"Doctor: {turnoCreado.Profesional?.NombreCompleto ?? "No disponible"}\n" +
                             $"Fecha: {FechaCalendar.SelectedDate.Value:dd/MM/yyyy}\n" +
                             $"Hora: {nuevoTurnoRequest.Hora}";

                MessageBox.Show(mensaje, "Turno Creado", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar formulario después de crear el turno
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el turno:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Rehabilitar el botón
                var botonCrear = sender as Button;
                if (botonCrear != null)
                {
                    botonCrear.IsEnabled = true;
                    botonCrear.Content = "Nuevo Turno";
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private int GetEspecialidadId(string especialidadNombre)
        {
            // Mapeo temporal de especialidades a IDs (en una implementación real esto vendría de la API)
            return especialidadNombre switch
            {
                "Cardiología" => 1,
                "Ginecología" => 2,
                "Pediatría" => 3,
                "Clínica General" => 4,
                _ => 1
            };
        }

        private int GetDoctorId(string doctorNombre)
        {
            // Mapeo temporal de doctores a IDs (en una implementación real esto vendría de la API)
            return doctorNombre switch
            {
                "Dr. Juan Pérez" => 1,
                "Dra. María García" => 2,
                "Dr. Carlos López" => 3,
                "Dra. Ana Martínez" => 4,
                _ => 1
            };
        }

        private void LimpiarFormulario()
        {
            NombreApellidoTextBox.Text = string.Empty;
            TelefonoTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            EspecialidadComboBox.SelectedIndex = 0;
            DoctorComboBox.SelectedIndex = 0;
            FechaCalendar.SelectedDate = DateTime.Today;
            HoraComboBox.SelectedIndex = 0;
            
            // Dar foco al primer campo
            NombreApellidoTextBox.Focus();
        }
    }
}
