using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;

namespace SaludTotal.Desktop.Views
{
    public partial class GestionProfesionalesWindow : Window
    {
        private readonly ApiService _apiService;
        private List<Profesional> _todosLosProfesionales = new List<Profesional>();
        private List<Profesional> _profesionalesFiltrados = new List<Profesional>();

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
            }
        }

        private async Task CargarTodosDoctoresConEspecialidadesAsync()
        {
            try
            {
                // Primero obtener todas las especialidades
                var especialidades = await _apiService.GetEspecialidadesAsync();
                var todosLosDoctores = new List<Profesional>();

                Console.WriteLine($"Especialidades encontradas: {especialidades.Count}");

                // Para cada especialidad, obtener sus doctores
                foreach (var especialidad in especialidades)
                {
                    try
                    {
                        var doctoresEspecialidad = await _apiService.GetDoctoresByEspecialidadAsync(especialidad.EspecialidadId);

                        foreach (var doctor in doctoresEspecialidad)
                        {
                            doctor.Especialidad.Nombre = especialidad.Nombre;
                            Console.WriteLine($"Asignando especialidad: '{doctor.Especialidad}' al doctor {doctor.NombreCompleto}");
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
                _profesionalesFiltrados = new List<Profesional>(_todosLosProfesionales);
                ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;

                Console.WriteLine($"Total de doctores cargados: {todosLosDoctores.Count}");
                foreach (var doctor in todosLosDoctores.Take(5))
                {
                    Console.WriteLine($"Doctor: {doctor.NombreCompleto}, Especialidad: '{doctor.Especialidad}' (Length: {doctor.Especialidad.Nombre?.Length})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CargarTodosDoctoresConEspecialidadesAsync: {ex.Message}");
                throw;
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
            var profesionalesFiltrados = new List<Profesional>(_todosLosProfesionales);

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
                    (p.NombreCompleto?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.NombreCompleto?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.Email?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.Telefono?.ToLower().Contains(terminoBusqueda) ?? false) ||
                    (p.Especialidad.Nombre?.ToLower().Contains(terminoBusqueda) ?? false)
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

        private List<Profesional> FiltrarPorEspecialidad(List<Profesional> profesionales, string especialidad)
        {
            if (especialidad == "Clínica General")
            {
                return profesionales.Where(p => 
                    p.Especialidad.Nombre?.Equals("Clínica General", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Especialidad.Nombre?.Equals("Clinica General", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Especialidad.Nombre?.Equals("Medicina General", StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            else
            {
                return profesionales.Where(p => 
                    p.Especialidad.Nombre?.Equals(especialidad, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Especialidad.Nombre?.Equals(especialidad.Replace("í", "i").Replace("é", "e"), StringComparison.OrdinalIgnoreCase) == true
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
            if (sender is Button button && button.Tag is Profesional profesional)
            {
                var detalleProfesionalWindow = new DetalleProfesionalWindow(profesional);
                detalleProfesionalWindow.Show();
                this.Close();
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
