using System.Windows;
using SaludTotal.Desktop.ViewModels;

namespace SaludTotal.Desktop.Views
{
    public partial class AdministracionTurnosWindow : Window
    {
        private readonly TurnosViewModel _viewModel;

        public AdministracionTurnosWindow()
        {
            InitializeComponent();
            _viewModel = new TurnosViewModel();
            this.DataContext = _viewModel;
        }

        #region Eventos de Ventana
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Eventos de Acciones
        private async void RefreshTurnos_Click(object sender, RoutedEventArgs e)
        {
            // Recargar los turnos desde la API
            await _viewModel.RecargarTurnosAsync();
        }

        private void NuevoTurno_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar funcionalidad para crear nuevo turno
            MessageBox.Show("Funcionalidad 'Nuevo Turno' estará disponible próximamente.", 
                           "Información", 
                           MessageBoxButton.OK, 
                           MessageBoxImage.Information);
        }

        #region Eventos de Filtros por Especialidad
        private async void FiltrarCardiologia_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== CLICK EN CARDIOLOGÍA ===");
            await _viewModel.FiltrarTurnosPorEspecialidadAsync("Cardiología");
        }

        private async void FiltrarGinecologia_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== CLICK EN GINECOLOGÍA ===");
            await _viewModel.FiltrarTurnosPorEspecialidadAsync("Ginecología");
        }

        private async void FiltrarPediatria_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== CLICK EN PEDIATRÍA ===");
            await _viewModel.FiltrarTurnosPorEspecialidadAsync("Pediatría");
        }

        private async void FiltrarClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== CLICK EN CLÍNICA GENERAL ===");
            await _viewModel.FiltrarTurnosPorEspecialidadAsync("Clínica General");
        }

        private async void FiltrarTodos_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== CLICK EN TODOS ===");
            await _viewModel.FiltrarTurnosPorEspecialidadAsync("Todos");
        }
        #endregion
        #endregion

        #region Eventos del Buscador
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await RealizarBusqueda();
        }

        private async void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await RealizarBusqueda();
            }
        }

        private void SearchTypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel != null && SearchTypeCombo.SelectedItem != null)
            {
                var selectedItem = (System.Windows.Controls.ComboBoxItem)SearchTypeCombo.SelectedItem;
                var tipoBusqueda = selectedItem.Content?.ToString()?.ToLower() ?? "doctor";
                
                // Mapear los valores del ComboBox a los valores esperados por la API
                _viewModel.TipoBusqueda = tipoBusqueda switch
                {
                    "doctor" => "doctor",
                    "paciente" => "paciente", 
                    "fecha" => "fecha",
                    _ => "doctor"
                };
                
                Console.WriteLine($"Tipo de búsqueda cambiado a: {_viewModel.TipoBusqueda}");
            }
        }

        private async void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                SearchTextBox.Text = string.Empty;
                await _viewModel.LimpiarBusquedaAsync();
            }
        }

        private async Task RealizarBusqueda()
        {
            if (_viewModel != null)
            {
                _viewModel.TerminoBusqueda = SearchTextBox.Text?.Trim() ?? string.Empty;
                await _viewModel.BuscarTurnosAsync();
            }
        }
        #endregion
    }
}
