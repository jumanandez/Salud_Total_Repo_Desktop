using SaludTotal.Models;
using SaludTotal.Desktop.Services;
using SaludTotal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using SaludTotal.Services;

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
                Console.WriteLine($"ACTUALIZACIÓN ESPECIALIDAD: Anterior='{_especialidadSeleccionada}', Nueva='{value}'");
                _especialidadSeleccionada = value;
                OnPropertyChanged();
                Console.WriteLine($"DESPUÉS OnPropertyChanged: EspecialidadSeleccionada='{_especialidadSeleccionada}'");
            }
        }
        private string _estadoSeleccionado = "Todos";
        public string EstadoSeleccionado
        {
            get { return _estadoSeleccionado; }
            set
            {
                _estadoSeleccionado = value;
                OnPropertyChanged();
            }
        }

        // Propiedades para el buscador
        private string _terminoBusqueda = string.Empty;
        public string TerminoBusqueda
        {
            get { return _terminoBusqueda; }
            set
            {
                _terminoBusqueda = value;
                OnPropertyChanged();
            }
        }

        private string _tipoBusqueda = "doctor";
        public string TipoBusqueda
        {
            get { return _tipoBusqueda; }
            set
            {
                _tipoBusqueda = value;
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
        /// Filtra turnos usando todos los filtros posibles (especialidad, fecha, doctor, paciente)
        /// </summary>
        public async Task FiltrarTurnosAsync(string? especialidad = null, string? fecha = null, string? doctor = null, string? paciente = null, string? estado = null)
        {
            IsLoading = true;
            try
            {
                var listaTurnos = await _apiService.GetTurnosAsync(especialidad, fecha, doctor, paciente, estado);
                Turnos = new ObservableCollection<Turno>(listaTurnos);
                EspecialidadSeleccionada = especialidad ?? "Todos";
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    OnPropertyChanged(nameof(EspecialidadSeleccionada));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener turnos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            await FiltrarTurnosAsync();
        }

        private async void CargarTurnos()
        {
            await RecargarTurnosAsync();
        }

        /// <summary>
        /// Filtra turnos por especialidad (para los botones de filtro)
        /// </summary>
        public async Task FiltrarTurnosPorEspecialidadAsync(string especialidad)
        {
            await FiltrarTurnosAsync(especialidad: especialidad == "Todos" ? null : especialidad);
        }

        /// <summary>
        /// Filtra turnos por estado (para el ComboBox de estado)
        /// </summary>
        public async Task FiltrarTurnosPorEstadoAsync(string estado)
        {
            // Si el estado es "Todos", no filtra por estado
            string? estadoFiltro = estado == "Todos" ? null : estado;
            // Mantener el filtro de especialidad actual
            string? especialidad = EspecialidadSeleccionada == "Todos" ? null : EspecialidadSeleccionada;
            var listaTurnos = await _apiService.GetTurnosAsync(especialidad, null, null, null); // Trae todos los turnos filtrados por especialidad
            if (estadoFiltro != null)
            {
                if (Enum.TryParse<EstadoTurno>(estadoFiltro, true, out var estadoEnum))
                {
                    listaTurnos = listaTurnos.Where(t => t.Estado == estadoEnum).ToList();
                }
            }
            Turnos = new ObservableCollection<Turno>(listaTurnos);
            EstadoSeleccionado = estado;
        }

        /// <summary>
        /// Limpia la búsqueda y muestra todos los turnos
        /// </summary>
        public async Task LimpiarBusquedaAsync()
        {
            TerminoBusqueda = string.Empty;
            await RecargarTurnosAsync();
        }
        public async Task CancelarTurno()
        {
            if (TurnoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un turno para cancelar.", "Acción requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultadoApi result = await _apiService.CancelarTurnoAsync(TurnoSeleccionado.Id);
            if (result.Success)
            {
                MessageBox.Show("Turno cancelado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await RecargarTurnosAsync();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al cancelar el turno.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}