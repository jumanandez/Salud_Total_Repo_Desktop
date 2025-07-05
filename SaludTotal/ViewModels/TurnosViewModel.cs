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
                Console.WriteLine($"Filtrando por especialidad: {especialidad}");
                List<Turno> listaTurnos;
                
                if (especialidad == "Todos")
                {
                    listaTurnos = await _apiService.GetTurnosAsync();
                    Console.WriteLine($"Obtenidos {listaTurnos.Count} turnos totales");
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

                    Console.WriteLine($"Mapeando {especialidad} a ID: {especialidadId}");

                    if (especialidadId > 0)
                    {
                        listaTurnos = await _apiService.GetTurnosPorEspecialidadAsync(especialidadId);
                        Console.WriteLine($"Obtenidos {listaTurnos.Count} turnos para {especialidad}");
                    }
                    else
                    {
                        listaTurnos = new List<Turno>();
                        Console.WriteLine($"Especialidad {especialidad} no reconocida");
                    }
                }

                // Debug: verificar qué datos estamos obteniendo
                foreach (var turno in listaTurnos.Take(3)) // Solo los primeros 3 para no saturar el log
                {
                    Console.WriteLine($"Turno {turno.Id}: " +
                        $"Paciente={turno.Paciente?.NombreCompleto ?? "NULL"}, " +
                        $"Profesional={turno.Profesional?.NombreCompleto ?? "NULL"}, " +
                        $"Fecha={turno.Fecha}, Estado={turno.Estado}");
                }

                Turnos = new ObservableCollection<Turno>(listaTurnos);
                
                // IMPORTANTE: Actualizar la especialidad seleccionada DESPUÉS de cargar los datos
                EspecialidadSeleccionada = especialidad;
                
                // Forzar notificación adicional para asegurar que el UI se actualice
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    OnPropertyChanged(nameof(EspecialidadSeleccionada));
                    Console.WriteLine($"FORZANDO ACTUALIZACIÓN UI: EspecialidadSeleccionada='{EspecialidadSeleccionada}'");
                });
#pragma warning restore CS4014
                
                Console.WriteLine($"EspecialidadSeleccionada actualizada a: {EspecialidadSeleccionada}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                Turnos = new ObservableCollection<Turno>();
                // Mantener la especialidad seleccionada para que el UI refleje el último intento
                EspecialidadSeleccionada = especialidad;
                
                // Forzar notificación también en caso de error
#pragma warning disable CS4014
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    OnPropertyChanged(nameof(EspecialidadSeleccionada));
                });
#pragma warning restore CS4014
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                MessageBox.Show($"No se pudieron cargar los turnos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Turnos = new ObservableCollection<Turno>();
                // Mantener la especialidad seleccionada para que el UI refleje el último intento
                EspecialidadSeleccionada = especialidad;
                
                // Forzar notificación también en caso de error
#pragma warning disable CS4014
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    OnPropertyChanged(nameof(EspecialidadSeleccionada));
                });
#pragma warning restore CS4014
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
            await FiltrarTurnosPorEspecialidadAsync("Todos");
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
                await RecargarTurnosAsync();
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
                await RecargarTurnosAsync();
            }
            else
            {
                MessageBox.Show("Ocurrió un error al cancelar el turno.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}