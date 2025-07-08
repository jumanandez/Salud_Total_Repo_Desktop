using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
                // Cargar todos los doctores al inicio
                var doctores = await _apiService.GetTodosDoctoresAsync();
                _todosLosProfesionales.Clear();
                
                foreach (var doctor in doctores)
                {
                    _todosLosProfesionales.Add(doctor);
                }

                _profesionalesFiltrados = new ObservableCollection<DoctorDto>(_todosLosProfesionales);
                ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los profesionales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Cargar datos de ejemplo como fallback
                LoadSampleData();
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
        }

        private async void FiltrarGinecologia_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Ginecología");
        }

        private async void FiltrarPediatria_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Pediatría");
        }

        private async void FiltrarClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Clínica General");
        }

        private async void FiltrarTodos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Recargar todos los doctores desde la API
                var doctores = await _apiService.GetTodosDoctoresAsync();
                _profesionalesFiltrados.Clear();
                
                foreach (var doctor in doctores)
                {
                    _profesionalesFiltrados.Add(doctor);
                }
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
            }
        }

        private async Task FiltrarPorEspecialidadAsync(string especialidad)
        {
            try
            {
                // Primero obtener las especialidades para conseguir el ID
                var especialidades = await _apiService.GetEspecialidadesAsync();
                var especialidadObj = especialidades.FirstOrDefault(e => e.Nombre.Equals(especialidad, StringComparison.OrdinalIgnoreCase));
                
                if (especialidadObj != null)
                {
                    // Obtener doctores por especialidad específica
                    var doctores = await _apiService.GetDoctoresByEspecialidadAsync(especialidadObj.Id);
                    _profesionalesFiltrados.Clear();
                    
                    foreach (var doctor in doctores)
                    {
                        // Asegurar que la especialidad esté asignada
                        doctor.Especialidad = especialidad;
                        _profesionalesFiltrados.Add(doctor);
                    }
                }
                else
                {
                    MessageBox.Show($"No se encontró la especialidad: {especialidad}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar por {especialidad}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback a filtro local
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales.Where(p => p.Especialidad.Equals(especialidad, StringComparison.OrdinalIgnoreCase)))
                {
                    _profesionalesFiltrados.Add(profesional);
                }
            }
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
    }
}
