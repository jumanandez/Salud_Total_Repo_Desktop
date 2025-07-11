using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace SaludTotal.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para InformesEmpresaWindow.xaml
    /// </summary>
    public partial class InformesEmpresaWindow : Window
    {
        private readonly ApiService _apiService;
        private SaludTotal.Models.EstadisticasGlobalesDto? _estadisticasActuales;

        public InformesEmpresaWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            
            // Configurar fechas por defecto (último mes)
            FechaHasta.SelectedDate = DateTime.Now;
            FechaDesde.SelectedDate = DateTime.Now.AddMonths(-1);
            
            // Cargar estadísticas iniciales
            _ = CargarEstadisticasGlobales();
        }

        private async Task CargarEstadisticasGlobales()
        {
            try
            {
                DateTime? fechaDesde = FechaDesde.SelectedDate;
                DateTime? fechaHasta = FechaHasta.SelectedDate;
                
                var estadisticasBackend = await _apiService.GetEstadisticasGlobalesAsync(fechaDesde, fechaHasta);
                
                if (estadisticasBackend != null)
                {
                    // Convertir del DTO del ApiService al DTO completo de Models
                    _estadisticasActuales = estadisticasBackend;
                    
                    ActualizarInterfaz();
                }
                else
                {
                    // Usar valores por defecto si no hay datos del backend
                    CargarEstadisticasPorDefecto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas globales: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                
                // Cargar estadísticas por defecto en caso de error
                CargarEstadisticasPorDefecto();
            }
        }

        private void CargarEstadisticasPorDefecto()
        {
            // Usar valores por defecto cuando no hay conexión al backend
            TotalDoctores.Text = "N/A";
            TotalPacientes.Text = "N/A";
            TotalTurnos.Text = "N/A";
            TurnosAtendidos.Text = "N/A";
            TurnosCancelados.Text = "N/A";
            TurnosRechazados.Text = "N/A";
            TurnosReprogramados.Text = "N/A";
            TurnosAceptados.Text = "N/A";
            TurnosDesaprovechados.Text = "N/A";
            PromedioTurnosPorDoctor.Text = "N/A";
            PorcentajeEficiencia.Text = "N/A";
        }

        private void ActualizarInterfaz()
        {
            if (_estadisticasActuales == null) return;

            TotalDoctores.Text = _estadisticasActuales.TotalDoctores.ToString();
            TotalPacientes.Text = _estadisticasActuales.TotalPacientes.ToString();
            TotalTurnos.Text = _estadisticasActuales.TotalTurnos.ToString();
            TurnosAtendidos.Text = _estadisticasActuales.TurnosAtendidos.ToString();
            TurnosCancelados.Text = _estadisticasActuales.TurnosCancelados.ToString();
            TurnosRechazados.Text = _estadisticasActuales.TurnosRechazados.ToString();
            TurnosReprogramados.Text = _estadisticasActuales.TurnosReprogramados.ToString();
            TurnosAceptados.Text = _estadisticasActuales.TurnosAceptados.ToString();
            TurnosDesaprovechados.Text = _estadisticasActuales.TurnosDesaprovechados.ToString();
        }

        private async void ActualizarEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            await CargarEstadisticasGlobales();
        }

        private async void ExportarEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"EstadisticasGlobales_{DateTime.Now:yyyyMMdd}.pdf",
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await GenerarPDFEstadisticas(saveFileDialog.FileName);
                    MessageBox.Show($"PDF exportado exitosamente a: {saveFileDialog.FileName}", 
                        "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Task GenerarPDFEstadisticas(string rutaArchivo)
        {
            return Task.Run(() =>
            {
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(rutaArchivo, FileMode.Create));
                
                document.Open();

                // Fuentes
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, BaseColor.BLACK);
                Font statFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.DARK_GRAY);

                // Título del documento
                Paragraph title = new Paragraph("Estadísticas Globales de la Empresa", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Línea separadora
                document.Add(new Paragraph("_____________________________________________________________________________", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                });

                // Período de las estadísticas
                string periodo = "";
                if (_estadisticasActuales?.FechaDesde != null && _estadisticasActuales?.FechaHasta != null)
                {
                    periodo = $"Período: {_estadisticasActuales.FechaDesde.Value:dd/MM/yyyy} - {_estadisticasActuales.FechaHasta.Value:dd/MM/yyyy}";
                }
                else
                {
                    periodo = $"Período: {FechaDesde.SelectedDate?.ToString("dd/MM/yyyy") ?? "N/A"} - {FechaHasta.SelectedDate?.ToString("dd/MM/yyyy") ?? "N/A"}";
                }
                
                document.Add(new Paragraph(periodo, headerFont) { 
                    Alignment = Element.ALIGN_CENTER, 
                    SpacingAfter = 30f 
                });

                // Crear tabla para las estadísticas
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SpacingAfter = 20f;

                // Headers de la tabla
                PdfPCell headerCell1 = new PdfPCell(new Phrase("Estadística", headerFont));
                headerCell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                headerCell1.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell1.Padding = 10f;
                table.AddCell(headerCell1);

                PdfPCell headerCell2 = new PdfPCell(new Phrase("Valor", headerFont));
                headerCell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                headerCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell2.Padding = 10f;
                table.AddCell(headerCell2);

                // Agregar estadísticas a la tabla
                if (_estadisticasActuales != null)
                {
                    AgregarFilaEstadistica(table, "Total de Doctores", _estadisticasActuales.TotalDoctores.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Total de Pacientes", _estadisticasActuales.TotalPacientes.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Total de Turnos", _estadisticasActuales.TotalTurnos.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Turnos Atendidos", _estadisticasActuales.TurnosAtendidos.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Turnos Cancelados", _estadisticasActuales.TurnosCancelados.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Turnos Rechazados", _estadisticasActuales.TurnosRechazados.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Turnos Reprogramados", _estadisticasActuales.TurnosReprogramados.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Turnos Aceptados", _estadisticasActuales.TurnosAceptados.ToString(), normalFont, statFont);
                    AgregarFilaEstadistica(table, "Turnos Desaprovechados", _estadisticasActuales.TurnosDesaprovechados.ToString(), normalFont, statFont);
                }

                document.Add(table);

                // Fecha de generación
                document.Add(new Paragraph($"Reporte generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 30f
                });

                document.Close();
            });
        }

        private void AgregarFilaEstadistica(PdfPTable table, string nombreEstadistica, string valor, Font normalFont, Font statFont)
        {
            PdfPCell cellNombre = new PdfPCell(new Phrase(nombreEstadistica, normalFont));
            cellNombre.HorizontalAlignment = Element.ALIGN_LEFT;
            cellNombre.Padding = 8f;
            table.AddCell(cellNombre);

            PdfPCell cellValor = new PdfPCell(new Phrase(valor, statFont));
            cellValor.HorizontalAlignment = Element.ALIGN_CENTER;
            cellValor.Padding = 8f;
            table.AddCell(cellValor);
        }

        private void VolverMenu_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana del menú de informes
            var informesMenuWindow = new InformesMenuWindow();
            informesMenuWindow.Show();
            
            // Cerrar la ventana actual
            this.Close();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}