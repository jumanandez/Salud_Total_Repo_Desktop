using System;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SaludTotal.Models;
using System.Windows.Threading;

// Enum para los días de la semana (Lunes=1, ..., Domingo=7)
public enum DiasSemana
{
    Lunes = 1,
    Martes = 2,
    Miercoles = 3,
    Jueves = 4,
    Viernes = 5,
    Sabado = 6,
    Domingo = 7
}

namespace SaludTotal.Desktop.Views
{
    public partial class NuevoTurnoWindow : Window
    {
        private readonly ApiService _apiService;
        private DatosFormularioResponse _datosFormulario;
        private List<SaludTotal.Models.Paciente> _pacientesEncontrados;
        private SaludTotal.Models.Paciente? _pacienteSeleccionado;
        private DispatcherTimer? _busquedaTimer;

        public NuevoTurnoWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _datosFormulario = new DatosFormularioResponse();
            _pacientesEncontrados = new List<SaludTotal.Models.Paciente>();
            
            // Configurar timer para búsqueda de pacientes
            _busquedaTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // Esperar 500ms después del último carácter
            };
            _busquedaTimer.Tick += BusquedaTimer_Tick;
            
            // Inicializar ComboBox de pacientes
            PacienteComboBox.Items.Clear();
            PacienteComboBox.Items.Add("Escriba para buscar pacientes...");
            PacienteComboBox.SelectedIndex = 0;
            
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
                EspecialidadComboBox.IsEnabled = false;
                DoctorComboBox.IsEnabled = false;
                HoraComboBox.IsEnabled = false;

                // Obtener especialidades
                var especialidades = await _apiService.GetEspecialidadesAsync();
                _datosFormulario.Especialidades = especialidades.Select(e => new Especialidad { EspecialidadId = e.EspecialidadId, Nombre = e.Nombre }).ToList();
                CargarEspecialidades();

                EspecialidadComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar especialidades:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    Tag = especialidad.EspecialidadId
                });
            }

            EspecialidadComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Carga los doctores de la especialidad seleccionada.
        /// </summary>
        private async Task CargarDoctoresPorEspecialidadAsync(int especialidadId)
        {
            DoctorComboBox.Items.Clear();
            DoctorComboBox.IsEnabled = false;
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Cargando doctores...", IsEnabled = false });

            try
            {
                var doctores = await _apiService.GetDoctoresByEspecialidadAsync(especialidadId);
                DoctorComboBox.Items.Clear();
                DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Seleccionar doctor...", Tag = -1, IsEnabled = false });
                foreach (var doctor in doctores)
                {
                    DoctorComboBox.Items.Add(new ComboBoxItem { Content = doctor.NombreCompleto, Tag = doctor.Id });
                }
                DoctorComboBox.SelectedIndex = 0;
                DoctorComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar doctores:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DoctorComboBox.Items.Clear();
                DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Error al cargar doctores", IsEnabled = false });
                DoctorComboBox.SelectedIndex = 0;
            }
            LimpiarHorarios();
        }

        /// <summary>
        /// Carga horarios laborales disponibles para el doctor seleccionado.
        /// </summary>
        private async Task CargarHorariosLaboralesAsync(int doctorId)
        {
            try
            {
                var horarios = await _apiService.GetHorariosLaboralesAsync(doctorId);
                // Mostrar los días y horarios laborales en un TextBlock (por ejemplo: DiasLaboralesTextBlock)
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
                MessageBox.Show($"Error al cargar días laborales:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Carga los slots de turnos disponibles para el doctor y fecha seleccionados.
        /// </summary>
        private async Task CargarSlotsTurnosDisponiblesAsync(int doctorId, DateTime fecha)
        {
            HoraComboBox.Items.Clear();
            HoraComboBox.IsEnabled = false;
            HoraComboBox.Items.Add(new ComboBoxItem { Content = "Cargando turnos...", IsEnabled = false });
            try
            {
                var slots = await _apiService.GetSlotsTurnosDisponiblesAsync(doctorId, fecha.ToString("yyyy-MM-dd"));
                HoraComboBox.Items.Clear();
                HoraComboBox.Items.Add(new ComboBoxItem { Content = "Seleccionar hora...", Tag = "", IsEnabled = false });
                foreach (var slot in slots)
                {
                    HoraComboBox.Items.Add(new ComboBoxItem { Content = slot.Hora, Tag = slot.Hora });
                }
                HoraComboBox.SelectedIndex = 0;
                HoraComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar turnos disponibles:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CargarHorariosEstaticos();
            }
        }

        /// <summary>
        /// Evento cuando cambia la selección de especialidad.
        /// </summary>
        private void EspecialidadComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EspecialidadComboBox.SelectedItem is ComboBoxItem item && item.Tag is int especialidadId && especialidadId > 0)
            {
                _ = CargarDoctoresPorEspecialidadAsync(especialidadId);
            }
        }

        /// <summary>
        /// Evento cuando cambia la selección de doctor.
        /// </summary>
        private void DoctorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorComboBox.SelectedItem is ComboBoxItem item && item.Tag is int doctorId && doctorId > 0)
            {
                _ = CargarHorariosLaboralesAsync(doctorId);
            }
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
                // Obtener días laborales del doctor
                var horarios = await _apiService.GetHorariosLaboralesAsync(doctorId);
                var diaSeleccionado = (int)FechaCalendar.SelectedDate.Value.DayOfWeek;
                // Ajustar para que Lunes=1 ... Domingo=7
                diaSeleccionado = diaSeleccionado == 0 ? 7 : diaSeleccionado;
                if (horarios.Any(h => h.DiaSemana == diaSeleccionado))
                {
                    await CargarSlotsTurnosDisponiblesAsync(doctorId, FechaCalendar.SelectedDate.Value);
                }
                else
                {
                    LimpiarHorarios();
                    HoraComboBox.Items.Clear();
                    HoraComboBox.Items.Add(new ComboBoxItem { Content = "El doctor no trabaja este día", IsEnabled = false });
                    HoraComboBox.SelectedIndex = 0;
                }
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
            // Validar que se haya seleccionado un paciente
            if (_pacienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, busque y seleccione un paciente.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                BuscarPacienteTextBox.Focus();
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

            // Mapear especialidades y doctores a IDs desde los datos dinámicos
            var especialidadId = GetEspecialidadIdFromSelection();
            var doctorId = GetDoctorIdFromSelection();
            var horaSeleccionada = GetHoraFromSelection();
            var especialidadNombre = (EspecialidadComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            var doctorNombre = (DoctorComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            var fechaSeleccionada = FechaCalendar.SelectedDate.Value.ToString("yyyy/MM/dd");

            if (especialidadId == -1 || doctorId == -1 || string.IsNullOrEmpty(horaSeleccionada))
            {
                MessageBox.Show("Por favor, complete todos los campos del formulario.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crear el turno directamente
            try
            {
                var request = new SaludTotal.Desktop.Services.NuevoTurnoRequest
                {
                    PacienteId = _pacienteSeleccionado?.Id ?? 0,
                    DoctorId = doctorId,
                    Fecha = FechaCalendar.SelectedDate.Value.ToString("yyyy-MM-dd"),
                    Hora = horaSeleccionada
                };
                
                await _apiService.CrearTurnoAsync(request);
                MessageBox.Show("Turno creado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el turno:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            BuscarPacienteTextBox.Text = string.Empty;
            PacienteComboBox.Items.Clear();
            PacienteComboBox.Items.Add("Escriba para buscar pacientes...");
            _pacientesEncontrados.Clear();
            _pacienteSeleccionado = null;
            
            EspecialidadComboBox.SelectedIndex = 0;
            DoctorComboBox.SelectedIndex = 0;
            FechaCalendar.SelectedDate = DateTime.Today;
            HoraComboBox.SelectedIndex = 0;
            
            // Dar foco al primer campo
            BuscarPacienteTextBox.Focus();
        }

        /// <summary>
        /// Evento del timer para búsqueda de pacientes.
        /// </summary>
        private async void BusquedaTimer_Tick(object? sender, EventArgs e)
        {
            if (_busquedaTimer != null)
            {
                _busquedaTimer.Stop();
                await BuscarPacientesAsync();
            }
        }

        /// <summary>
        /// Evento cuando cambia el texto de búsqueda de paciente.
        /// </summary>
        private void BuscarPacienteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_busquedaTimer != null)
            {
                _busquedaTimer.Stop();
                _busquedaTimer.Start();
            }
        }

        /// <summary>
        /// Evento cuando cambia la selección del paciente.
        /// </summary>
        private void PacienteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PacienteComboBox.SelectedItem is SaludTotal.Models.Paciente paciente)
            {
                _pacienteSeleccionado = paciente;
            }
            else
            {
                _pacienteSeleccionado = null;
            }
        }

        /// <summary>
        /// Busca pacientes basado en el texto de búsqueda.
        /// </summary>
        private async Task BuscarPacientesAsync()
        {
            try
            {
                string query = BuscarPacienteTextBox.Text?.Trim() ?? "";
                
                if (string.IsNullOrWhiteSpace(query))
                {
                    PacienteComboBox.Items.Clear();
                    PacienteComboBox.Items.Add("Escriba para buscar pacientes...");
                    _pacientesEncontrados.Clear();
                    return;
                }

                if (query.Length < 2)
                {
                    return; // No buscar con menos de 2 caracteres
                }

                PacienteComboBox.IsEnabled = false;
                PacienteComboBox.Items.Clear();
                PacienteComboBox.Items.Add("Buscando...");

                _pacientesEncontrados = await _apiService.BuscarPacientesAsync(query);

                PacienteComboBox.Items.Clear();
                
                if (_pacientesEncontrados.Any())
                {
                    PacienteComboBox.Items.Add("Seleccione un paciente...");
                    foreach (var paciente in _pacientesEncontrados)
                    {
                        PacienteComboBox.Items.Add(paciente);
                    }
                }
                else
                {
                    PacienteComboBox.Items.Add("No se encontraron pacientes");
                }

                PacienteComboBox.SelectedIndex = 0;
                PacienteComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PacienteComboBox.Items.Clear();
                PacienteComboBox.Items.Add("Error en la búsqueda");
                PacienteComboBox.IsEnabled = true;
                MessageBox.Show($"Error al buscar pacientes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Implementación básica para compilar y fallback visual
        private void CargarDatosEstaticos()
        {
            EspecialidadComboBox.Items.Clear();
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Cardiología", Tag = 1 });
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Ginecología", Tag = 2 });
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Pediatría", Tag = 3 });
            EspecialidadComboBox.Items.Add(new ComboBoxItem { Content = "Clínica General", Tag = 4 });
            EspecialidadComboBox.SelectedIndex = 0;

            DoctorComboBox.Items.Clear();
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dr. Juan Pérez", Tag = 1 });
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dra. María García", Tag = 2 });
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dr. Carlos López", Tag = 3 });
            DoctorComboBox.Items.Add(new ComboBoxItem { Content = "Dra. Ana Martínez", Tag = 4 });
            DoctorComboBox.SelectedIndex = 0;

            CargarHorariosEstaticos();
        }

        private void LimpiarHorarios()
        {
            HoraComboBox.Items.Clear();
            HoraComboBox.Items.Add(new ComboBoxItem { Content = "Seleccione doctor y fecha primero", IsEnabled = false });
            HoraComboBox.SelectedIndex = 0;
        }

        private void CargarHorariosEstaticos()
        {
            HoraComboBox.Items.Clear();
            var horas = new[] { "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00" };
            foreach (var hora in horas)
            {
                HoraComboBox.Items.Add(new ComboBoxItem { Content = hora, Tag = hora });
            }
            HoraComboBox.SelectedIndex = 0;
        }
    }
}
