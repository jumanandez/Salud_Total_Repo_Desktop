using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using SaludTotal.Models;
using SaludTotal.Desktop.Services;
using System.Threading.Tasks;

namespace SaludTotal.Desktop.Views
{
    public partial class EstadisticasDoctorWindow : Window
    {
        private Profesional _doctor;
        private List<Turno> _todosTurnos = new List<Turno>();
        private readonly ApiService _apiService;

        // Estadísticas calculadas
        private int _totalTurnos;
        private int _turnosAtendidos;
        private int _turnosCancelados;
        private int _turnosRechazados;
        private int _turnosReprogramados;
        private int _turnosAceptados;
        private int _turnosDesaprovechados;
        
        // Estadísticas de ausencias
        private int _diasAusencia;
        private int _turnosPerdidosAusencia;
        private int _ausenciasAnotadas;
        private DateTime? _ultimaAusenciaInicio;
        private DateTime? _ultimaAusenciaFin;

        public EstadisticasDoctorWindow(Profesional doctor)
        {
            InitializeComponent();
            _doctor = doctor;
            _apiService = new ApiService();
            
            // Configurar fechas por defecto (último mes)
            FechaHasta.SelectedDate = DateTime.Now;
            FechaDesde.SelectedDate = DateTime.Now.AddMonths(-1);
            
            CargarDatosDoctor();
            _ = CargarEstadisticas();
        }

        private void CargarDatosDoctor()
        {
            DoctorNombre.Text = _doctor.NombreCompleto;
            
            // Usar datos del DoctorDto si están disponibles, sino valores por defecto
            if (_doctor != null)
            {
                DoctorTelefono.Text = !string.IsNullOrEmpty(_doctor.Telefono) ? _doctor.Telefono : "No disponible";
                DoctorEmail.Text = !string.IsNullOrEmpty(_doctor.Email) ? _doctor.Email : "No disponible";
            }
            else
            {
                DoctorTelefono.Text = "No disponible";
                DoctorEmail.Text = "No disponible";
            }
            
            DoctorEspecialidad.Text = _doctor.Especialidad?.Nombre ?? "No especificada";
        }

        private async Task CargarEstadisticas()
        {
            try
            {
                // Usar estadísticas reales del backend
                DateTime? fechaDesde = FechaDesde.SelectedDate;
                DateTime? fechaHasta = FechaHasta.SelectedDate;
                
                var estadisticasBackend = await _apiService.GetEstadisticasDoctorAsync(_doctor.Id, fechaDesde, fechaHasta);
                
                if (estadisticasBackend != null && estadisticasBackend.DoctorId > 0)
                {
                    // Usar estadísticas del backend
                    CargarEstadisticasDesdeBackend(estadisticasBackend);
                }
                else
                {
                    return;
                }
                
                // Actualizar UI
                ActualizarInterfaz();
            }
            catch (Exception ex)
            {
                // En caso de error con el backend, intentar el fallback
                try
                {
                    _todosTurnos = await _apiService.ObtenerTurnosPorDoctor(_doctor.Id);
                    ActualizarInterfaz();
                }
                catch (Exception fallbackEx)
                {
                    MessageBox.Show($"Error al cargar estadísticas: {ex.Message}\nError fallback: {fallbackEx.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ActualizarInterfaz()
        {
            // Actualizar estadísticas de turnos
            TurnosAtendidos.Text = _turnosAtendidos.ToString();
            TurnosCancelados.Text = _turnosCancelados.ToString();
            TurnosRechazados.Text = _turnosRechazados.ToString();
            TurnosReprogramados.Text = _turnosReprogramados.ToString();
            TurnosAceptados.Text = _turnosAceptados.ToString();
            TurnosDesaprovechados.Text = _turnosDesaprovechados.ToString();
            AusenciasAnotadas.Text = _ausenciasAnotadas.ToString();
            // Actualizar estadísticas de ausencias
            DiasAusencia.Text = _diasAusencia.ToString();
            TurnosPerdidosAusencia.Text = _turnosPerdidosAusencia.ToString();
            if (_ultimaAusenciaInicio.HasValue && _ultimaAusenciaFin.HasValue)
            {
                UltimaAusencia.Text = $"{_ultimaAusenciaInicio:dd/MM/yyyy} - {_ultimaAusenciaFin:dd/MM/yyyy}";
            }
            else if (_ultimaAusenciaInicio.HasValue)
            {
                UltimaAusencia.Text = _ultimaAusenciaInicio.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                UltimaAusencia.Text = "N/A";
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FiltroFecha_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // El método ActualizarEstadisticas_Click se encargará de actualizar las estadísticas
        }

        private async void ActualizarEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            await CargarEstadisticas();
        }

        private async void ExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Configurar el diálogo de guardar archivo
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Stats{_doctor.NombreCompleto.Replace(" ", "")}.pdf",
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Capturar los datos en el hilo principal antes de pasar al task
                    var fechaRango = "";
                    if (FechaDesde.SelectedDate.HasValue && FechaHasta.SelectedDate.HasValue)
                    {
                        fechaRango = $"_{FechaDesde.SelectedDate.Value:yyyyMMdd}-{FechaHasta.SelectedDate.Value:yyyyMMdd}";
                    }
                    
                    var datosParaPDF = new
                    {
                        NombreDoctor = _doctor.NombreCompleto,
                        Especialidad = _doctor.Especialidad?.Nombre ?? "No especificada",
                        TotalTurnos = _totalTurnos,
                        TurnosAtendidos = _turnosAtendidos,
                        TurnosCancelados = _turnosCancelados,
                        TurnosRechazados = _turnosRechazados,
                        TurnosReprogramados = _turnosReprogramados,
                        TurnosAceptados = _turnosAceptados,
                        TurnosDesaprovechados = _turnosDesaprovechados,
                        DiasAusencia = _diasAusencia,
                        TurnosPerdidosAusencia = _turnosPerdidosAusencia,
                        AusenciasAnotadas = _ausenciasAnotadas,
                        UltimaAusencia = "De: " + ((DateTime)_ultimaAusenciaInicio).ToString("dd/MM/yyyy") + " a: " + ((DateTime)_ultimaAusenciaFin).ToString("dd/MM/yyyy"),
                        FechaRango = fechaRango
                    };

                    await GenerarPDF(saveFileDialog.FileName, datosParaPDF);
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

        private Task GenerarPDF(string rutaArchivo, dynamic datosParaPDF)
        {
            return Task.Run(() =>
            {
                // Crear el documento PDF
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(rutaArchivo, FileMode.Create));
                
                document.Open();

                // Fuentes
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
                Font statFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);

                // Título del documento
                Paragraph title = new Paragraph("Estadísticas del Doctor", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Línea separadora
                document.Add(new Paragraph("_____________________________________________________________________________", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                });

                // Información del doctor
                document.Add(new Paragraph("Información del Doctor", headerFont) { SpacingAfter = 10f });
                
                document.Add(new Paragraph($"Nombre: {datosParaPDF.NombreDoctor}", normalFont) { SpacingAfter = 5f });
                document.Add(new Paragraph($"Especialidad: {datosParaPDF.Especialidad}", normalFont) { SpacingAfter = 20f });

                // Estadísticas
                document.Add(new Paragraph("Estadísticas de Turnos", headerFont) { SpacingAfter = 15f });

                // Crear tabla para las estadísticas
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SpacingAfter = 20f;

                // Headers de la tabla
                PdfPCell headerCell1 = new PdfPCell(new Phrase("Tipo de Turno", headerFont));
                headerCell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                headerCell1.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell1.Padding = 10f;
                table.AddCell(headerCell1);

                PdfPCell headerCell2 = new PdfPCell(new Phrase("Cantidad", headerFont));
                headerCell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                headerCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell2.Padding = 10f;
                table.AddCell(headerCell2);

                // Agregar estadísticas a la tabla
                AgregarFilaEstadistica(table, "Turnos Atendidos", datosParaPDF.TurnosAtendidos, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Cancelados", datosParaPDF.TurnosCancelados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Rechazados", datosParaPDF.TurnosRechazados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Reprogramados", datosParaPDF.TurnosReprogramados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Aceptados", datosParaPDF.TurnosAceptados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Desaprovechados", datosParaPDF.TurnosDesaprovechados, normalFont, statFont);

                document.Add(table);

                // Estadísticas de Ausencias
                document.Add(new Paragraph("Estadísticas de Ausencias", headerFont) { SpacingAfter = 15f });

                // Crear tabla para las ausencias
                PdfPTable ausenciasTable = new PdfPTable(2);
                ausenciasTable.WidthPercentage = 100;
                ausenciasTable.SpacingAfter = 20f;

                // Headers de la tabla de ausencias
                PdfPCell headerAusencia1 = new PdfPCell(new Phrase("Tipo de Ausencia", headerFont));
                headerAusencia1.BackgroundColor = BaseColor.LIGHT_GRAY;
                headerAusencia1.HorizontalAlignment = Element.ALIGN_CENTER;
                headerAusencia1.Padding = 10f;
                ausenciasTable.AddCell(headerAusencia1);

                PdfPCell headerAusencia2 = new PdfPCell(new Phrase("Valor", headerFont));
                headerAusencia2.BackgroundColor = BaseColor.LIGHT_GRAY;
                headerAusencia2.HorizontalAlignment = Element.ALIGN_CENTER;
                headerAusencia2.Padding = 10f;
                ausenciasTable.AddCell(headerAusencia2);

                // Agregar estadísticas de ausencias a la tabla
                AgregarFilaEstadistica(ausenciasTable, "Días de Ausencia", datosParaPDF.DiasAusencia, normalFont, statFont);
                AgregarFilaEstadistica(ausenciasTable, "Turnos Perdidos", datosParaPDF.TurnosPerdidosAusencia, normalFont, statFont);
                AgregarFilaEstadistica(ausenciasTable, "Ausencias Anotadas", datosParaPDF.AusenciasAnotadas, normalFont, statFont);
                AgregarFilaEstadisticaTexto(ausenciasTable, "Última Ausencia", datosParaPDF.UltimaAusencia, normalFont, statFont);

                document.Add(ausenciasTable);

                // Total de turnos
                int totalTurnos = datosParaPDF.TotalTurnos;
                
                document.Add(new Paragraph($"Total de Turnos: {totalTurnos}", headerFont) 
                { 
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20f 
                });

                // Fecha de generación
                document.Add(new Paragraph($"Reporte generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 30f
                });

                document.Close();
            });
        }

        private void AgregarFilaEstadistica(PdfPTable table, string tipoTurno, int cantidad, Font normalFont, Font statFont)
        {
            PdfPCell cellTipo = new PdfPCell(new Phrase(tipoTurno, normalFont));
            cellTipo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellTipo.Padding = 8f;
            table.AddCell(cellTipo);

            PdfPCell cellCantidad = new PdfPCell(new Phrase(cantidad.ToString(), statFont));
            cellCantidad.HorizontalAlignment = Element.ALIGN_CENTER;
            cellCantidad.Padding = 8f;
            table.AddCell(cellCantidad);
        }

        private void AgregarFilaEstadisticaTexto(PdfPTable table, string tipoTurno, string valor, Font normalFont, Font statFont)
        {
            PdfPCell cellTipo = new PdfPCell(new Phrase(tipoTurno, normalFont));
            cellTipo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellTipo.Padding = 8f;
            table.AddCell(cellTipo);

            PdfPCell cellValor = new PdfPCell(new Phrase(valor, statFont));
            cellValor.HorizontalAlignment = Element.ALIGN_CENTER;
            cellValor.Padding = 8f;
            table.AddCell(cellValor);
        }

        private void CargarEstadisticasDesdeBackend(SaludTotal.Models.EstadisticasDoctorDto estadisticas)
        {
            // Cargar estadísticas directamente desde el DTO del backend
            _totalTurnos = estadisticas.TotalTurnos;
            _turnosAtendidos = estadisticas.TurnosAtendidos;
            _turnosCancelados = estadisticas.TurnosCancelados;
            _turnosRechazados = estadisticas.TurnosRechazados;
            _turnosReprogramados = estadisticas.TurnosReprogramados;
            _turnosAceptados = estadisticas.TurnosAceptados;
            _turnosDesaprovechados = estadisticas.TurnosDesaprovechados;
            _ausenciasAnotadas = estadisticas.AusenciasAnotadas;
            _diasAusencia = estadisticas.DiasAusencia;
            // Adaptar para el nuevo modelo de UltimaAusencia (inicio y fin)
            if (estadisticas.UltimaAusencia != null)
            {
                _ultimaAusenciaInicio = estadisticas.UltimaAusencia.FechaInicio;
                _ultimaAusenciaFin = estadisticas.UltimaAusencia.FechaFin;
            }
            else
            {
                _ultimaAusenciaInicio = null;
                _ultimaAusenciaFin = null;
            }
        }
    }
}
