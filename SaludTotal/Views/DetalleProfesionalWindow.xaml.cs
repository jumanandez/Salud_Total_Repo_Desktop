using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;

namespace SaludTotal.Desktop.Views
{
    public partial class DetalleProfesionalWindow : Window
    {
        private Profesional _profesional;
        private List<AusenciaDto> _todasLasAusencias = new List<AusenciaDto>();
        private List<AusenciaDto> _ausenciasFiltradas = new List<AusenciaDto>();

        public DetalleProfesionalWindow(Profesional profesional)
        {
            InitializeComponent();
            _profesional = profesional;
            Loaded += DetalleProfesionalWindow_Loaded;
        }

        private void DetalleProfesionalWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatosProfesional();
            CargarAusenciasEjemplo();
        }

        private void CargarDatosProfesional()
        {
            NombreApellidoText.Text = _profesional.NombreCompleto;
            EmailText.Text = _profesional.Email ?? "No especificado";
            TelefonoText.Text = _profesional.Telefono ?? "No especificado";
            EspecialidadText.Text = _profesional.Especialidad != null ?
                _profesional.Especialidad.Nombre ?? "No especificada" : "No especificada";

            // Aplicar color de especialidad
            var border = EspecialidadText.Parent as Border;
            if (border != null)
            {
                if(_profesional.Especialidad == null || string.IsNullOrEmpty(_profesional.Especialidad.Nombre))
                {
                    border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(148, 163, 184)); // #94A3B8
                    return;
                }
                switch (_profesional.Especialidad.Nombre?.ToLower())
                {
                    case "cardiología":
                    case "cardiologia":
                        border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 107, 53)); // #FF6B35
                        break;
                    case "ginecología":
                    case "ginecologia":
                        border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(217, 70, 239)); // #D946EF
                        break;
                    case "pediatría":
                    case "pediatria":
                        border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(14, 165, 233)); // #0EA5E9
                        break;
                    case "clínica general":
                    case "clinica general":
                    case "medicina general":
                        border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129)); // #10B981
                        break;
                    default:
                        border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(148, 163, 184)); // #94A3B8
                        break;
                }
            }
        }

        private void CargarAusenciasEjemplo()
        {
            // Datos de ejemplo para ausencias
            _todasLasAusencias = new List<AusenciaDto>
            {
                new AusenciaDto 
                { 
                    Id = 1, 
                    FechaInicio = DateTime.Now.AddDays(-15), 
                    FechaFin = DateTime.Now.AddDays(-10), 
                    Dias = 5,
                    Motivo = "Cirugía menor - recuperación"
                },
                new AusenciaDto 
                { 
                    Id = 2, 
                    FechaInicio = DateTime.Now.AddDays(-60), 
                    FechaFin = DateTime.Now.AddDays(-46), 
                    Dias = 14,
                    Motivo = "Vacaciones anuales"
                },
                new AusenciaDto 
                { 
                    Id = 3, 
                    FechaInicio = DateTime.Now.AddDays(5), 
                    FechaFin = DateTime.Now.AddDays(7), 
                    Dias = 2,
                    Motivo = "Congreso de Cardiología 2025"
                },
                new AusenciaDto 
                { 
                    Id = 4, 
                    FechaInicio = DateTime.Now.AddDays(-3), 
                    FechaFin = DateTime.Now.AddDays(-3), 
                    Dias = 1,
                    Motivo = "Asuntos familiares"
                },
                new AusenciaDto 
                { 
                    Id = 5, 
                    FechaInicio = DateTime.Now.AddDays(10), 
                    FechaFin = DateTime.Now.AddDays(12), 
                    Dias = 3,
                    Motivo = "Consulta médica especializada"
                }
            };

            _ausenciasFiltradas = new List<AusenciaDto>(_todasLasAusencias);
            AusenciasDataGrid.ItemsSource = _ausenciasFiltradas;
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            var gestionProfesionalesWindow = new GestionProfesionalesWindow();
            gestionProfesionalesWindow.Show();
            this.Close();
        }

        private void NuevaAusencia_Click(object sender, RoutedEventArgs e)
        {
            // Por ahora solo mostrar un diálogo
            MessageBox.Show("Funcionalidad de Nueva Ausencia en desarrollo.\n\nEsta función permitirá:\n• Seleccionar tipo de ausencia\n• Establecer fechas\n• Agregar motivo\n• Enviar para aprobación", 
                          "Nueva Ausencia", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FiltrarAusencias_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var ausenciasFiltradas = new List<AusenciaDto>(_todasLasAusencias);

            // Filtrar por fecha desde
            if (FechaDesdeFilter.SelectedDate.HasValue)
            {
                ausenciasFiltradas = ausenciasFiltradas.Where(a => a.FechaInicio >= FechaDesdeFilter.SelectedDate.Value).ToList();
            }

            // Filtrar por fecha hasta
            if (FechaHastaFilter.SelectedDate.HasValue)
            {
                ausenciasFiltradas = ausenciasFiltradas.Where(a => a.FechaFin <= FechaHastaFilter.SelectedDate.Value).ToList();
            }

            _ausenciasFiltradas = ausenciasFiltradas;
            AusenciasDataGrid.ItemsSource = null;
            AusenciasDataGrid.ItemsSource = _ausenciasFiltradas;
        }

        private void EditarAusencia_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AusenciaDto ausencia)
            {
                MessageBox.Show($"Editar ausencia ID: {ausencia.Id}\nFecha: {ausencia.FechaInicio:dd/MM/yyyy} - {ausencia.FechaFin:dd/MM/yyyy}\nMotivo: {ausencia.Motivo}\n\nFuncionalidad en desarrollo.", 
                              "Editar Ausencia", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EliminarAusencia_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AusenciaDto ausencia)
            {
                var result = MessageBox.Show($"¿Está seguro de que desea eliminar la ausencia?\n\nFecha: {ausencia.FechaInicio:dd/MM/yyyy} - {ausencia.FechaFin:dd/MM/yyyy}\nMotivo: {ausencia.Motivo}", 
                                           "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Por ahora solo simular eliminación
                    _todasLasAusencias.Remove(ausencia);
                    AplicarFiltros();
                    MessageBox.Show("Ausencia eliminada correctamente.", "Eliminación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ResumenAusencias_Click(object sender, RoutedEventArgs e)
        {
            var totalAusencias = _todasLasAusencias.Count;
            var totalDias = _todasLasAusencias.Sum(a => a.Dias);

            MessageBox.Show($"Resumen de Ausencias - {_profesional.NombreCompleto}\n\n" +
                          $"Total de ausencias: {totalAusencias}\n" +
                          $"Total días de ausencia: {totalDias} días", 
                          "Resumen de Ausencias", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportarAusencias_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Exportar ausencias de {_profesional.NombreCompleto}\n\nFuncionalidad en desarrollo.\n\nEl archivo PDF incluirá:\n• Información del profesional\n• Lista completa de ausencias\n• Estadísticas y resumen", 
                          "Exportar a PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Clase para representar las ausencias
    public class AusenciaDto
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int Dias { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
