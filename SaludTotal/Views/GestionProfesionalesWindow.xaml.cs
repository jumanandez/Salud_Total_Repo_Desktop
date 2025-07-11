using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using SaludTotal.Desktop.Services;

namespace SaludTotal.Desktop.Views
{
    public partial class GestionProfesionalesWindow : Window
    {
        private readonly ApiService _apiService;
        private List<DoctorDto> _todosLosProfesionales = new List<DoctorDto>();
        private List<DoctorDto> _profesionalesFiltrados = new List<DoctorDto>();

        public GestionProfesionalesWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            Loaded += GestionProfesionalesWindow_Loaded;
        }

        private async void GestionProfesionalesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarProfesionales();
        }

        private async System.Threading.Tasks.Task CargarProfesionales()
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

        private async System.Threading.Tasks.Task CargarTodosDoctoresConEspecialidadesAsync()
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
                _todosLosProfesionales = todosLosDoctores;
                _profesionalesFiltrados = new List<DoctorDto>(_todosLosProfesionales);
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
            _todosLosProfesionales = new List<DoctorDto>
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

            _profesionalesFiltrados = new List<DoctorDto>(_todosLosProfesionales);
            ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VolverMenu_Click(object sender, RoutedEventArgs e)
        {
            var dashboardWindow = new DashboardWindow();
            dashboardWindow.Show();
            this.Close();
        }

        #region Funcionalidad de Búsqueda

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AplicarFiltrosYBusqueda();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltrosYBusqueda();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            AplicarFiltrosYBusqueda();
        }

        #endregion

        #region Filtros por Especialidad

        private void FiltrarCardiologia_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstilosBotonesFiltro("Cardiología");
            AplicarFiltrosYBusqueda();
        }

        private void FiltrarGinecologia_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstilosBotonesFiltro("Ginecología");
            AplicarFiltrosYBusqueda();
        }

        private void FiltrarPediatria_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstilosBotonesFiltro("Pediatría");
            AplicarFiltrosYBusqueda();
        }

        private void FiltrarClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstilosBotonesFiltro("Clínica General");
            AplicarFiltrosYBusqueda();
        }

        private void FiltrarTodos_Click(object sender, RoutedEventArgs e)
        {
            ActualizarEstilosBotonesFiltro("Todos");
            AplicarFiltrosYBusqueda();
        }

        private void AplicarFiltrosYBusqueda()
        {
            var profesionalesFiltrados = new List<DoctorDto>(_todosLosProfesionales);

            // Aplicar filtro de especialidad
            string especialidadActiva = ObtenerEspecialidadActiva();
            if (!string.IsNullOrEmpty(especialidadActiva) && especialidadActiva != "Todos")
            {
                profesionalesFiltrados = FiltrarPorEspecialidad(profesionalesFiltrados, especialidadActiva);
            }

            // Aplicar filtro de búsqueda
            string terminoBusqueda = SearchTextBox.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(terminoBusqueda))
            {
                profesionalesFiltrados = profesionalesFiltrados.Where(p => 
                    (p.NombreCompletoCalculado?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.NombreCompleto?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.Email?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.Telefono?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.Especialidad?.ToLower().Contains(terminoBusqueda) ?? false)
                ).ToList();
            }

            // Actualizar la lista filtrada y el DataGrid
            _profesionalesFiltrados = profesionalesFiltrados;
            ProfesionalesDataGrid.ItemsSource = null;
            ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
        }

        private string ObtenerEspecialidadActiva()
        {
            if (CardiologiaButton.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Cardiología";
            else if (GinecologiaButton.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Ginecología";
            else if (PediatriaButton.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Pediatría";
            else if (ClinicaGeneralButton.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Clínica General";
            else
                return "Todos";
        }

        private List<DoctorDto> FiltrarPorEspecialidad(List<DoctorDto> profesionales, string especialidad)
        {
            if (especialidad == "Clínica General")
            {
                return profesionales.Where(p => 
                    p.Especialidad?.Equals("Clínica General", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Especialidad?.Equals("Clinica General", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Especialidad?.Equals("Medicina General", StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            else
            {
                return profesionales.Where(p => 
                    p.Especialidad?.Equals(especialidad, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Especialidad?.Equals(especialidad.Replace("í", "i").Replace("é", "e"), StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
        }

        private void ActualizarEstilosBotonesFiltro(string filtroActivo)
        {
            // Resetear todos los botones al estilo normal
            CardiologiaButton.Style = (Style)FindResource("FilterButtonStyle");
            GinecologiaButton.Style = (Style)FindResource("FilterButtonStyle");
            PediatriaButton.Style = (Style)FindResource("FilterButtonStyle");
            ClinicaGeneralButton.Style = (Style)FindResource("FilterButtonStyle");
            TodosButton.Style = (Style)FindResource("FilterButtonStyle");

            // Aplicar estilo activo al botón correspondiente
            switch (filtroActivo)
            {
                case "Cardiología":
                    CardiologiaButton.Style = (Style)FindResource("ActiveFilterButtonStyle");
                    break;
                case "Ginecología":
                    GinecologiaButton.Style = (Style)FindResource("ActiveFilterButtonStyle");
                    break;
                case "Pediatría":
                    PediatriaButton.Style = (Style)FindResource("ActiveFilterButtonStyle");
                    break;
                case "Clínica General":
                    ClinicaGeneralButton.Style = (Style)FindResource("ActiveFilterButtonStyle");
                    break;
                case "Todos":
                    TodosButton.Style = (Style)FindResource("ActiveFilterButtonStyle");
                    break;
            }
        }

        #endregion

        #region Gestión de Profesionales

        private void GestionarProfesional_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is DoctorDto profesional)
            {
                // Por ahora solo mostramos un mensaje indicando que la funcionalidad está en desarrollo
                MessageBox.Show($"Gestión del profesional: {profesional.NombreCompletoCalculado}\n\nEsta funcionalidad está en desarrollo.", 
                              "Gestión de Profesional", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ActualizarLista_Click(object sender, RoutedEventArgs e)
        {
            await CargarProfesionales();
            
            // Resetear filtros
            SearchTextBox.Text = "";
            ActualizarEstilosBotonesFiltro("Todos");
            AplicarFiltrosYBusqueda();
        }

        #endregion
    }
}
