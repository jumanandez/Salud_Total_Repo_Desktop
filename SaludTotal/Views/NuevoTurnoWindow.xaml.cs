using System;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SaludTotal.Desktop.Views
{
    public partial class NuevoTurnoWindow : Window
    {
        private readonly ApiService _apiService;
        private DatosFormularioResponse _datosFormulario;

        public NuevoTurnoWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _datosFormulario = new DatosFormularioResponse();
            
            // Establecer fecha actual por defecto
            FechaCalendar.SelectedDate = DateTime.Today;
            
            // Cargar datos iniciales del formulario
            _ = CargarDatosFormularioAsync();
        }

        /// <summary>
        /// Carga los datos iniciales del formulario (especialidades y doctores).
        /// </summary>
        private async Task CargarDatosFormularioAsync()
        {
            try
            {
                // Mostrar indicador de carga
                EspecialidadComboBox.IsEnabled = false;
                DoctorComboBox.IsEnabled = false;
                HoraComboBox.IsEnabled = false;

                // Obtener datos del formulario
                _datosFormulario = await _apiService.GetDatosFormularioAsync();

                // Cargar especialidades
                CargarEspecialidades();
                
                // Habilitar controles
                EspecialidadComboBox.IsEnabled = true;
                DoctorComboBox.IsEnabled = true;
                HoraComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos del formulario:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // En caso de error, cargar datos estáticos como fallback
                CargarDatosEstaticos();
            }
        }

        /// <summary>
        /// Carga las especialidades en el ComboBox.
        /// </summary>
        private void CargarEspecialidades()
        {
            EspecialidadComboBox.Items.Clear();
            
            // Agregar elemento por defecto
            EspecialidadComboBox.Items.Add(new ComboBoxItem 
            { 
                Content = "Seleccionar especialidad...", 
                Tag = -1,
                IsEnabled = false
            });

            // Agregar especialidades desde la API
            foreach (var especialidad in _datosFormulario.Especialidades)
            {
                EspecialidadComboBox.Items.Add(new ComboBoxItem
                {
                    Content = especialidad.Nombre,
                    Tag = especialidad.Id
                });
            }

            EspecialidadComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Carga los doctores de la especialidad seleccionada.
        /// </summary>
        private void CargarDoctoresPorEspecialidad(int especialidadId)
        {
            DoctorComboBox.Items.Clear();
            
            // Agregar elemento por defecto
            DoctorComboBox.Items.Add(new ComboBoxItem 
            { 
                Content = "Seleccionar doctor...", 
                Tag = -1,
                IsEnabled = false
            });

            // Buscar doctores de la especialidad
            var grupoEspecialidad = _datosFormulario.DoctoresPorEspecialidad
                .FirstOrDefault(g => g.EspecialidadId == especialidadId);

            if (grupoEspecialidad != null)
            {
                foreach (var doctor in grupoEspecialidad.Doctores)
                {
                    DoctorComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = doctor.NombreCompleto,
                        Tag = doctor.Id
                    });
                }
            }

            DoctorComboBox.SelectedIndex = 0;
            
            // Limpiar horarios al cambiar especialidad
            LimpiarHorarios();
        }

        /// <summary>
        /// Carga horarios disponibles para el doctor y fecha seleccionados.
        /// </summary>
        private async Task CargarHorariosDisponiblesAsync(int doctorId, DateTime fecha)
        {
            try
            {
                HoraComboBox.IsEnabled = false;
                HoraComboBox.Items.Clear();
                
                // Agregar indicador de carga
                HoraComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = "Cargando horarios...", 
                    IsEnabled = false 
                });

                // Obtener horarios disponibles
                var datosConHorarios = await _apiService.GetDatosFormularioAsync(doctorId, fecha.ToString("yyyy-MM-dd"));
                
                HoraComboBox.Items.Clear();
                
                // Agregar elemento por defecto
                HoraComboBox.Items.Add(new ComboBoxItem 
                { 
                    Content = "Seleccionar hora...", 
                    Tag = "",
                    IsEnabled = false
                });

                // Agregar horarios disponibles
                if (datosConHorarios.HorariosDisponibles.Any())
                {
                    foreach (var horario in datosConHorarios.HorariosDisponibles.Where(h => h.Disponible))
                    {
                        HoraComboBox.Items.Add(new ComboBoxItem
                        {
                            Content = horario.Display,
                            Tag = horario.Hora
                        });
                    }
                }
                else
                {
                    HoraComboBox.Items.Add(new ComboBoxItem 
                    { 
                        Content = "No hay horarios disponibles", 
                        IsEnabled = false 
                    });
                }

                HoraComboBox.SelectedIndex = 0;
                HoraComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar horarios:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CargarHorariosEstaticos();
            }
        }

        /// <summary>
        /// Limpia los horarios disponibles.
        /// </summary>
        private void LimpiarHorarios()
        {
            HoraComboBox.Items.Clear();
            HoraComboBox.Items.Add(new ComboBoxItem 
            { 
                Content = "Seleccione doctor y fecha primero", 
                IsEnabled = false 
            });
            HoraComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Carga datos estáticos como fallback en caso de error de API.
        /// </summary>
        private void CargarDatosEstaticos()
        {
            // Cargar especialidades estáticas
            EspecialidadComboBox.Items.Clear();
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Cardiología", Tag = 1 });
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Ginecología", Tag = 2 });
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Pediatría", Tag = 3 });
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Clínica General", Tag = 4 });
            EspecialidadComboBox.SelectedIndex = 0;

            // Cargar doctores estáticos
            DoctorComboBox.Items.Clear();
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dr. Juan Pérez", Tag = 1 });
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dra. María García", Tag = 2 });
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dr. Carlos López", Tag = 3 });
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dra. Ana Martínez", Tag = 4 });
            DoctorComboBox.SelectedIndex = 0;

            CargarHorariosEstaticos();
        }

        /// <summary>
        /// Carga horarios estáticos como fallback.
        /// </summary>
        private void CargarHorariosEstaticos()
        {
            HoraComboBox.Items.Clear();
            var horas = new[] { "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", 
                               "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", 
                               "16:00", "16:30", "17:00", "17:30", "18:00" };

            foreach (var hora in horas)
            {
                HoraComboBox.Items.Add(new ComboBoxItem { Content = hora, Tag = hora });
            }
            HoraComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Evento cuando cambia la selección de especialidad.
        /// </summary>
        private void EspecialidadComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EspecialidadComboBox.SelectedItem is ComboBoxItem item && item.Tag is int especialidadId && especialidadId > 0)
            {
                CargarDoctoresPorEspecialidad(especialidadId);
            }
        }

        /// <summary>
        /// Evento cuando cambia la selección de doctor.
        /// </summary>
        private void DoctorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarHorariosSegunSeleccion();
        }

        /// <summary>
        /// Evento cuando cambia la fecha seleccionada.
        /// </summary>
        private void FechaCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarHorariosSegunSeleccion();
        }

        /// <summary>
        /// Actualiza los horarios cuando cambia el doctor o la fecha.
        /// </summary>
        private async void ActualizarHorariosSegunSeleccion()
        {
            if (DoctorComboBox.SelectedItem is ComboBoxItem doctorItem && 
                doctorItem.Tag is int doctorId && doctorId > 0 &&
                FechaCalendar.SelectedDate.HasValue)
            {
                await CargarHorariosDisponiblesAsync(doctorId, FechaCalendar.SelectedDate.Value);
            }
            else
            {
                LimpiarHorarios();
            }
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

                // Mapear especialidades y doctores a IDs desde los datos dinámicos
                var especialidadId = GetEspecialidadIdFromSelection();
                var doctorId = GetDoctorIdFromSelection();
                var horaSeleccionada = GetHoraFromSelection();
                
                if (especialidadId == -1 || doctorId == -1 || string.IsNullOrEmpty(horaSeleccionada))
                {
                    MessageBox.Show("Por favor, complete todos los campos del formulario.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Crear el objeto de solicitud
                var nuevoTurnoRequest = new NuevoTurnoRequest
                {
                    PacienteNombreApellido = NombreApellidoTextBox.Text.Trim(),
                    PacienteTelefono = string.IsNullOrWhiteSpace(TelefonoTextBox.Text) ? null : TelefonoTextBox.Text.Trim(),
                    PacienteEmail = EmailTextBox.Text.Trim(),
                    DoctorId = doctorId,
                    Fecha = FechaCalendar.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Hora = horaSeleccionada,
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

        /// <summary>
        /// Obtiene el ID de la especialidad seleccionada.
        /// </summary>
        private int GetEspecialidadIdFromSelection()
        {
            if (EspecialidadComboBox.SelectedItem is ComboBoxItem item && item.Tag is int id)
            {
                return id;
            }
            return -1;
        }

        /// <summary>
        /// Obtiene el ID del doctor seleccionado.
        /// </summary>
        private int GetDoctorIdFromSelection()
        {
            if (DoctorComboBox.SelectedItem is ComboBoxItem item && item.Tag is int id)
            {
                return id;
            }
            return -1;
        }

        /// <summary>
        /// Obtiene la hora seleccionada.
        /// </summary>
        private string GetHoraFromSelection()
        {
            if (HoraComboBox.SelectedItem is ComboBoxItem item && item.Tag is string hora)
            {
                return hora;
            }
            return string.Empty;
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
