using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.ViewModels;
using SaludTotal.Models;

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
            // Abrir la ventana de Nuevo Turno
            var nuevoTurnoWindow = new NuevoTurnoWindow();
            nuevoTurnoWindow.Show();
            this.Close();
        }

        private async void GestionarTurno_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el turno del botón clickeado
            var button = sender as Button;
            var turno = button?.Tag as Turno;
            
            if (turno != null)
            {
                // Abrir la ventana de gestión de turnos con los detalles del turno
                var detalleTurnoWindow = new DetalleTurnoWindow(turno);
                var resultado = detalleTurnoWindow.ShowDialog();
                
                // Si se confirmaron cambios en el turno, actualizar la lista automáticamente
                if (resultado == true && detalleTurnoWindow.Confirmado)
                {
                    await _viewModel.RecargarTurnosAsync();
                }
            }
        }
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
                // Usar el método de filtrado centralizado
                string? especialidad = null, fecha = null, doctor = null, paciente = null, estado = null;
                switch (_viewModel.TipoBusqueda.ToLower())
                {
                    case "doctor": doctor = _viewModel.TerminoBusqueda; break;
                    case "paciente": paciente = _viewModel.TerminoBusqueda; break;
                    case "fecha": fecha = _viewModel.TerminoBusqueda; break;
                    case "especialidad": especialidad = _viewModel.TerminoBusqueda; break;
                    case "estado": estado = _viewModel.TerminoBusqueda; break;
                }
                await _viewModel.FiltrarTurnosAsync(especialidad, fecha, doctor, paciente, estado);
            }
        }
        #endregion

        // Evento para el ComboBox de especialidad
        private async void EspecialidadComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel != null && EspecialidadComboBox.SelectedValue != null)
            {
                string especialidad = EspecialidadComboBox.SelectedValue.ToString() ?? "Todos";
                await _viewModel.FiltrarTurnosPorEspecialidadAsync(especialidad);
            }
        }

        // Evento para el ComboBox de estado
        private async void EstadoComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel != null && EstadoComboBox.SelectedValue != null)
            {
                string estado = EstadoComboBox.SelectedValue.ToString() ?? "Todos";
                string? especialidad = _viewModel.EspecialidadSeleccionada == "Todos" ? null : _viewModel.EspecialidadSeleccionada;
                string? estadoFiltro = estado == "Todos" ? null : estado;
                // Llama a FiltrarTurnosAsync usando el estado como filtro
                await _viewModel.FiltrarTurnosAsync(especialidad, null, null, null, estadoFiltro);
                _viewModel.EstadoSeleccionado = estado;
            }
        }

        private void VolverInicio_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana Dashboard
            var dashboardWindow = new DashboardWindow();
            dashboardWindow.Show();
            
            // Cerrar la ventana actual
            this.Close();
        }

        #region Eventos para Solicitudes de Reprogramación
        private void AceptarSolicitud_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var solicitud = button?.Tag as SolicitudReprogramacion;
            return;
        }

        private void RechazarSolicitud_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var solicitud = button?.Tag as SolicitudReprogramacion;

            return;
        }
        #endregion
    }
}
