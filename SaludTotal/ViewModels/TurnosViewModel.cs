using SaludTotal.Models;
using SaludTotal.Desktop.Services;
using SaludTotal.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace SaludTotal.Desktop.ViewModels
{
    public class TurnosViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;


        private ObservableCollection<Turno> _turnos;
        public ObservableCollection<Turno> Turnos
        {
            get { return _turnos; }
            set
            {
                _turnos = value;
                OnPropertyChanged();
            }
        }

        private Turno _turnoSeleccionado;
        public Turno TurnoSeleccionado
        {
            get { return _turnoSeleccionado; }
            set
            {
                _turnoSeleccionado = value;
                OnPropertyChanged();
                // Opcional: podrías querer que los comandos se re-evalúen aquí.
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // --- Constructor ---
        public TurnosViewModel()
        {
            _apiService = new ApiService();
            Turnos = new ObservableCollection<Turno>();

            // Cargamos los datos al iniciar el ViewModel
            CargarTurnos();
        }

        // --- Lógica de Comandos ---

        private async void CargarTurnos()
        {
            IsLoading = true;
            try
            {
                var listaTurnos = await _apiService.GetTurnosAsync();
                Turnos = new ObservableCollection<Turno>(listaTurnos);
            }
            catch (Exception ex)
            {
                // Manejo de errores de cara al usuario
                MessageBox.Show($"No se pudieron cargar los turnos: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ConfirmarTurno()
        {
            if (TurnoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un turno para confirmar.", "Acción requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = await _apiService.ConfirmarTurnoAsync(TurnoSeleccionado.Id);
            if (success)
            {
                MessageBox.Show("Turno confirmado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                // Refrescamos la lista para ver el cambio de estado.
                CargarTurnos();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al confirmar el turno.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CancelarTurno()
        {
            if (TurnoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un turno para cancelar.", "Acción requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = await _apiService.CancelarTurnoAsync(TurnoSeleccionado.Id);
            if (success)
            {
                MessageBox.Show("Turno cancelado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                // Refrescamos la lista.
                CargarTurnos();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al cancelar el turno.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}