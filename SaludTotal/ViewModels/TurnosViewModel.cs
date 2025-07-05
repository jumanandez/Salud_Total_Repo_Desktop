using SaludTotal.Models;
using SaludTotal.Desktop.Services;
using SaludTotal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace SaludTotal.Desktop.ViewModels
{
    public class TurnosViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;


        private ObservableCollection<Turno> _turnos = new ObservableCollection<Turno>();
        public ObservableCollection<Turno> Turnos
        {
            get { return _turnos; }
            set
            {
                _turnos = value;
                OnPropertyChanged();
            }
        }

        private Turno? _turnoSeleccionado;
        public Turno? TurnoSeleccionado
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

        private string _especialidadSeleccionada = "Todos";
        public string EspecialidadSeleccionada
        {
            get { return _especialidadSeleccionada; }
            set
            {
                _especialidadSeleccionada = value;
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

        /// <summary>
        /// Filtra turnos por especialidad específica
        /// </summary>
        /// <param name="especialidad">Nombre de la especialidad</param>
        public async Task FiltrarTurnosPorEspecialidadAsync(string especialidad)
        {
            IsLoading = true;
            try
            {
                List<Turno> listaTurnos;
                
                if (especialidad == "Todos")
                {
                    listaTurnos = await _apiService.GetTurnosAsync();
                }
                else
                {
                    // Mapear especialidades a IDs (esto debería venir de la base de datos idealmente)
                    int especialidadId = especialidad switch
                    {
                        "Cardiología" => 1,
                        "Ginecología" => 2,
                        "Pediatría" => 3,
                        "Clínica General" => 4,
                        _ => 0
                    };

                    if (especialidadId > 0)
                    {
                        listaTurnos = await _apiService.GetTurnosPorEspecialidadAsync(especialidadId);
                    }
                    else
                    {
                        listaTurnos = new List<Turno>();
                    }
                }

                Turnos = new ObservableCollection<Turno>(listaTurnos);
                EspecialidadSeleccionada = especialidad;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                Turnos = new ObservableCollection<Turno>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los turnos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Turnos = new ObservableCollection<Turno>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Recarga todos los turnos desde la API
        /// </summary>
        public async Task RecargarTurnosAsync()
        {
            await FiltrarTurnosPorEspecialidadAsync("Todos");
        }

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