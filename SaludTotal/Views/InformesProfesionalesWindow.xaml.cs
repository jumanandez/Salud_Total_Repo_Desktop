using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.Services;
using System;
using System.Threading.Tasks;

namespace SaludTotal.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para InformesProfesionalesWindow.xaml
    /// </summary>
    public partial class InformesProfesionalesWindow : Window
    {
        private ObservableCollection<DoctorDto> _todosLosProfesionales = new();
        private ObservableCollection<DoctorDto> _profesionalesFiltrados = new();
        private readonly ApiService _apiService;

        public InformesProfesionalesWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            LoadProfesionalesData();
        }

        private async void LoadProfesionalesData()
        {
            try
            {
                // Cargar todos los doctores con sus especialidades
                await CargarTodosDoctoresConEspecialidadesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los profesionales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Cargar datos de ejemplo como fallback
                LoadSampleData();
            }
        }

        private async Task CargarTodosDoctoresConEspecialidadesAsync()
        {
            try
            {
                // Primero obtener todas las especialidades
                var especialidades = await _apiService.GetEspecialidadesAsync();
                var todosLosDoctores = new List<DoctorDto>();

                Console.WriteLine($"Especialidades encontradas: {especialidades.Count}");

                // Para cada especialidad, obtener sus doctores
                foreach (var especialidad in especialidades)
                {
                    try
                    {
                        var doctoresEspecialidad = await _apiService.GetDoctoresByEspecialidadAsync(especialidad.Id);
                        
                        foreach (var doctor in doctoresEspecialidad)
                        {
                            doctor.Especialidad = especialidad.Nombre;
                            Console.WriteLine($"Asignando especialidad: '{doctor.Especialidad}' al doctor {doctor.NombreCompletoCalculado}");
                            todosLosDoctores.Add(doctor);
                        }
                        
                        Console.WriteLine($"Especialidad '{especialidad.Nombre}': {doctoresEspecialidad.Count} doctores");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar doctores de {especialidad.Nombre}: {ex.Message}");
                    }
                }

                // Actualizar las colecciones
                _todosLosProfesionales.Clear();
                foreach (var doctor in todosLosDoctores)
                {
                    _todosLosProfesionales.Add(doctor);
                }

                _profesionalesFiltrados = new ObservableCollection<DoctorDto>(_todosLosProfesionales);
                ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
                
                Console.WriteLine($"Total de doctores cargados: {todosLosDoctores.Count}");
                foreach (var doctor in todosLosDoctores.Take(5))
                {
                    Console.WriteLine($"Doctor: {doctor.NombreCompletoCalculado}, Especialidad: '{doctor.Especialidad}' (Length: {doctor.Especialidad?.Length})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CargarTodosDoctoresConEspecialidadesAsync: {ex.Message}");
                throw;
            }
        }

        private void LoadSampleData()
        {
            // Datos de ejemplo como fallback
            _todosLosProfesionales = new ObservableCollection<DoctorDto>
            {
                new DoctorDto { Id = 1, NombreCompleto = "Dr. Juan Carlos Pérez", Email = "juan.perez@saludtotal.com", Telefono = "+54 11 1234-5678", Especialidad = "Cardiología" },
                new DoctorDto { Id = 2, NombreCompleto = "Dra. María Elena García", Email = "maria.garcia@saludtotal.com", Telefono = "+54 11 2345-6789", Especialidad = "Ginecología" },
                new DoctorDto { Id = 3, NombreCompleto = "Dr. Roberto Martínez", Email = "roberto.martinez@saludtotal.com", Telefono = "+54 11 3456-7890", Especialidad = "Pediatría" },
                new DoctorDto { Id = 4, NombreCompleto = "Dra. Ana Sofía López", Email = "ana.lopez@saludtotal.com", Telefono = "+54 11 4567-8901", Especialidad = "Clínica General" },
                new DoctorDto { Id = 5, NombreCompleto = "Dr. Carlos Eduardo Ruiz", Email = "carlos.ruiz@saludtotal.com", Telefono = "+54 11 5678-9012", Especialidad = "Cardiología" },
                new DoctorDto { Id = 6, NombreCompleto = "Dra. Laura Patricia Mendoza", Email = "laura.mendoza@saludtotal.com", Telefono = "+54 11 6789-0123", Especialidad = "Ginecología" },
                new DoctorDto { Id = 7, NombreCompleto = "Dr. Fernando Andrés Silva", Email = "fernando.silva@saludtotal.com", Telefono = "+54 11 7890-1234", Especialidad = "Pediatría" },
                new DoctorDto { Id = 8, NombreCompleto = "Dra. Isabel Cristina Torres", Email = "isabel.torres@saludtotal.com", Telefono = "+54 11 8901-2345", Especialidad = "Clínica General" }
            };

            _profesionalesFiltrados = new ObservableCollection<DoctorDto>(_todosLosProfesionales);
            ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
        }

        private void VolverMenu_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana del menú de informes
            var informesMenuWindow = new InformesMenuWindow();
            informesMenuWindow.Show();
            
            // Cerrar la ventana actual
            this.Close();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void FiltrarCardiologia_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Cardiología");
            ActualizarEstadoFiltros("Cardiología");
        }

        private async void FiltrarGinecologia_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Ginecología");
            ActualizarEstadoFiltros("Ginecología");
        }

        private async void FiltrarPediatria_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Pediatría");
            ActualizarEstadoFiltros("Pediatría");
        }

        private async void FiltrarClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Clínica General");
            ActualizarEstadoFiltros("Clínica General");
        }

        private async void FiltrarTodos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Recargar todos los doctores con sus especialidades
                await CargarTodosDoctoresConEspecialidadesAsync();
                
                // Actualizar el estado visual del filtro
                ActualizarEstadoFiltros("Todos");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar todos los doctores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback a mostrar todos los que ya tenemos cargados
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales)
                {
                    _profesionalesFiltrados.Add(profesional);
                }
                ActualizarEstadoFiltros("Todos");
            }
        }

        private async Task FiltrarPorEspecialidadAsync(string especialidad)
        {
            try
            {
                // Primero obtener las especialidades para conseguir el ID
                var especialidades = await _apiService.GetEspecialidadesAsync();
                
                // Debug: Mostrar especialidades disponibles
                Console.WriteLine($"Especialidades disponibles:");
                foreach (var esp in especialidades)
                {
                    Console.WriteLine($"  - ID: {esp.Id}, Nombre: '{esp.Nombre}'");
                }
                
                // Buscar la especialidad de manera flexible (sin tildes, case insensitive)
                var especialidadObj = especialidades.FirstOrDefault(e => 
                    NormalizarTexto(e.Nombre).Equals(NormalizarTexto(especialidad), StringComparison.OrdinalIgnoreCase));
                
                if (especialidadObj != null)
                {
                    Console.WriteLine($"Especialidad encontrada: {especialidadObj.Nombre} (ID: {especialidadObj.Id})");
                    
                    // Obtener doctores por especialidad específica
                    var doctores = await _apiService.GetDoctoresByEspecialidadAsync(especialidadObj.Id);
                    _profesionalesFiltrados.Clear();
                    
                    foreach (var doctor in doctores)
                    {
                        // Asegurar que la especialidad esté asignada con el nombre original de la API
                        doctor.Especialidad = especialidadObj.Nombre;
                        _profesionalesFiltrados.Add(doctor);
                    }
                    
                    Console.WriteLine($"Doctores filtrados: {doctores.Count}");
                }
                else
                {
                    // Si no encontramos la especialidad en la API, intentar filtro local
                    Console.WriteLine($"Especialidad '{especialidad}' no encontrada en API, intentando filtro local");
                    
                    var doctoresFiltrados = _todosLosProfesionales.Where(p => 
                        NormalizarTexto(p.Especialidad).Contains(NormalizarTexto(especialidad), StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    _profesionalesFiltrados.Clear();
                    foreach (var doctor in doctoresFiltrados)
                    {
                        _profesionalesFiltrados.Add(doctor);
                    }
                    
                    if (doctoresFiltrados.Count == 0)
                    {
                        MessageBox.Show($"No se encontraron doctores para la especialidad: {especialidad}\n\nEspecialidades disponibles:\n{string.Join(", ", especialidades.Select(e => e.Nombre))}", 
                                      "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar por {especialidad}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback a filtro local
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales.Where(p => 
                    NormalizarTexto(p.Especialidad).Contains(NormalizarTexto(especialidad), StringComparison.OrdinalIgnoreCase)))
                {
                    _profesionalesFiltrados.Add(profesional);
                }
            }
        }

        /// <summary>
        /// Normaliza texto removiendo tildes y caracteres especiales para comparaciones flexibles
        /// </summary>
        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            
            return texto
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace("ñ", "n").Replace("Ñ", "N")
                .Trim();
        }

        private void GenerarInforme_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar generación de informe
            MessageBox.Show("Generando informe de profesionales - Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportarDatos_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar exportación de datos
            MessageBox.Show("Exportando datos - Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Mantener compatibilidad con métodos anteriores
        private void Cardiologia_Click(object sender, RoutedEventArgs e)
        {
            FiltrarCardiologia_Click(sender, e);
        }

        private void Ginecologia_Click(object sender, RoutedEventArgs e)
        {
            FiltrarGinecologia_Click(sender, e);
        }

        private void Pediatria_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPediatria_Click(sender, e);
        }

        private void ClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            FiltrarClinicaGeneral_Click(sender, e);
        }

        private void Siguiente_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar funcionalidad del botón Siguiente
            MessageBox.Show("Funcionalidad del botón Siguiente en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Actualiza el estado visual de los botones de filtro
        /// </summary>
        private void ActualizarEstadoFiltros(string filtroActivo)
        {
            // Resetear todos los botones a su estilo normal
            var todosButton = this.FindName("TodosButton") as Button;
            var cardiologiaButton = this.FindName("CardiologiaButton") as Button;
            var ginecologiaButton = this.FindName("GinecologiaButton") as Button;
            var pediatriaButton = this.FindName("PediatriaButton") as Button;
            var clinicaGeneralButton = this.FindName("ClinicaGeneralButton") as Button;

            // Aplicar estilo normal a todos
            if (todosButton != null) todosButton.Style = (Style)FindResource("FilterButtonStyle");
            if (cardiologiaButton != null) cardiologiaButton.Style = (Style)FindResource("FilterButtonStyle");
            if (ginecologiaButton != null) ginecologiaButton.Style = (Style)FindResource("FilterButtonStyle");
            if (pediatriaButton != null) pediatriaButton.Style = (Style)FindResource("FilterButtonStyle");
            if (clinicaGeneralButton != null) clinicaGeneralButton.Style = (Style)FindResource("FilterButtonStyle");

            // Aplicar estilo activo al filtro seleccionado
            Style activeStyle = (Style)FindResource("ActiveFilterButtonStyle");
            
            switch (filtroActivo)
            {
                case "Todos":
                    if (todosButton != null) todosButton.Style = activeStyle;
                    break;
                case "Cardiología":
                    if (cardiologiaButton != null) cardiologiaButton.Style = activeStyle;
                    break;
                case "Ginecología":
                    if (ginecologiaButton != null) ginecologiaButton.Style = activeStyle;
                    break;
                case "Pediatría":
                    if (pediatriaButton != null) pediatriaButton.Style = activeStyle;
                    break;
                case "Clínica General":
                    if (clinicaGeneralButton != null) clinicaGeneralButton.Style = activeStyle;
                    break;
            }
        }
    }
}
