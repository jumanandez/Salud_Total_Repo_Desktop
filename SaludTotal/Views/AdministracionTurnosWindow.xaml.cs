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

        private async void TestConexion_Click(object sender, RoutedEventArgs e)
        {
            var apiService = new SaludTotal.Desktop.Services.ApiService();
            string resultado = await apiService.TestConexionAsync();
            MessageBox.Show(resultado, "Prueba de Conexión", MessageBoxButton.OK, 
                           resultado.Contains("✅") ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        #endregion
        #endregion
    }
}
