using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;

namespace SaludTotal.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para GenerarInformeDoctoresWindow.xaml
    /// </summary>
    public partial class GenerarInformeDoctoresWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<SaludTotal.Models.EstadisticasDoctorDto> _estadisticasDoctores = new();

        public GenerarInformeDoctoresWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            // Obtener referencias a los controles definidos en XAML
            fechaDesdePicker = this.FindName("FechaDesdePicker") as System.Windows.Controls.DatePicker;
            fechaHastaPicker = this.FindName("FechaHastaPicker") as System.Windows.Controls.DatePicker;
            // Establecer valores iniciales: desde un mes atrás hasta hoy
            if (fechaDesdePicker != null)
                fechaDesdePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            if (fechaHastaPicker != null)
                fechaHastaPicker.SelectedDate = DateTime.Today;
            // Cargar estadísticas iniciales
            _ = BuscarEstadisticasAsync(fechaDesdePicker?.SelectedDate, fechaHastaPicker?.SelectedDate);
        }

        // Referencias a los controles de fecha
        private System.Windows.Controls.DatePicker? fechaDesdePicker;
        private System.Windows.Controls.DatePicker? fechaHastaPicker;

        // Aquí irán los métodos para manejar la búsqueda, filtrado y carga de estadísticas
        // Ejemplo de método base para buscar estadísticas (a completar):
        private async Task BuscarEstadisticasAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var lista = await _apiService.GetEstadisticasTodosDoctoresAsync(fechaInicio, fechaFin);
                _estadisticasDoctores = new ObservableCollection<SaludTotal.Models.EstadisticasDoctorDto>(lista.estadisticasDoctorDtos);
                var dataGrid = this.FindName("EstadisticasDataGrid") as System.Windows.Controls.DataGrid;
                if (dataGrid != null)
                    dataGrid.ItemsSource = _estadisticasDoctores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar estadísticas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento para el botón Buscar
        private async void Buscar_Click(object sender, RoutedEventArgs e)
        {
            DateTime? fechaDesde = fechaDesdePicker?.SelectedDate;
            DateTime? fechaHasta = fechaHastaPicker?.SelectedDate;
            await BuscarEstadisticasAsync(fechaDesde, fechaHasta);
        }

        // Evento para el botón Exportar Informe
        private void ExportarInforme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"InformeDoctores_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = "pdf"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    GenerarPDFDoctores(saveFileDialog.FileName, _estadisticasDoctores);
                    MessageBox.Show($"PDF exportado exitosamente a: {saveFileDialog.FileName}", "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarPDFDoctores(string rutaArchivo, ObservableCollection<SaludTotal.Models.EstadisticasDoctorDto> estadisticas)
        {
            try
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 50, 50, 50, 50);
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new System.IO.FileStream(rutaArchivo, System.IO.FileMode.Create));
                document.Open();

                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 18, iTextSharp.text.BaseColor.DARK_GRAY);
                iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 10, iTextSharp.text.BaseColor.BLACK);
                iTextSharp.text.Font normalFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 9, iTextSharp.text.BaseColor.BLACK);

                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Informe de Estadísticas de Doctores", titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                document.Add(new iTextSharp.text.Paragraph($"Total de doctores: {estadisticas.Count}", headerFont) { SpacingAfter = 20f });

                iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(13);
                table.WidthPercentage = 100;
                table.SpacingAfter = 20f;
                float[] columnWidths = { 1.2f, 2.2f, 2.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 2.2f, 1.2f };
                table.SetWidths(columnWidths);

                AgregarHeaderTabla(table, "ID", headerFont);
                AgregarHeaderTabla(table, "Doctor", headerFont);
                AgregarHeaderTabla(table, "Especialidad", headerFont);
                AgregarHeaderTabla(table, "Total Turnos", headerFont);
                AgregarHeaderTabla(table, "Atendidos", headerFont);
                AgregarHeaderTabla(table, "Cancelados", headerFont);
                AgregarHeaderTabla(table, "Rechazados", headerFont);
                AgregarHeaderTabla(table, "Aceptados", headerFont);
                AgregarHeaderTabla(table, "Desaprovechados", headerFont);
                AgregarHeaderTabla(table, "Reprogramados", headerFont);
                AgregarHeaderTabla(table, "Ausencias", headerFont);
                AgregarHeaderTabla(table, "Última Ausencia", headerFont);
                AgregarHeaderTabla(table, "Días Ausencia", headerFont);

                foreach (var est in estadisticas)
                {
                    AgregarCeldaTabla(table, est.DoctorId.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.NombreDoctor ?? "", normalFont);
                    AgregarCeldaTabla(table, est.Especialidad ?? "", normalFont);
                    AgregarCeldaTabla(table, est.TotalTurnos.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.TurnosAtendidos.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.TurnosCancelados.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.TurnosRechazados.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.TurnosAceptados.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.TurnosDesaprovechados.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.TurnosReprogramados.ToString(), normalFont);
                    AgregarCeldaTabla(table, est.AusenciasAnotadas.ToString(), normalFont);
                    string ultimaAusencia = est.UltimaAusencia != null ? $"{est.UltimaAusencia.FechaInicio:yyyy-MM-dd} a {est.UltimaAusencia.FechaFin:yyyy-MM-dd}" : "-";
                    AgregarCeldaTabla(table, ultimaAusencia, normalFont);
                    AgregarCeldaTabla(table, est.DiasAusencia.ToString(), normalFont);
                }

                document.Add(table);

                document.Add(new iTextSharp.text.Paragraph($"Reporte generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont)
                {
                    Alignment = iTextSharp.text.Element.ALIGN_RIGHT,
                    SpacingBefore = 30f
                });

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar PDF: {ex.Message}");
            }
        }

        private void AgregarHeaderTabla(iTextSharp.text.pdf.PdfPTable table, string texto, iTextSharp.text.Font font)
        {
            var headerCell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(texto, font));
            headerCell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            headerCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            headerCell.Padding = 8f;
            table.AddCell(headerCell);
        }

        private void AgregarCeldaTabla(iTextSharp.text.pdf.PdfPTable table, string texto, iTextSharp.text.Font font)
        {
            var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(texto, font));
            cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
            cell.Padding = 6f;
            table.AddCell(cell);
        }
    }
}
