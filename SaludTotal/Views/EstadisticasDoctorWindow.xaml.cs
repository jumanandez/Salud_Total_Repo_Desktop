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
        private int _turnosAtendidos;
        private int _turnosCancelados;
        private int _turnosRechazados;
        private int _turnosReprogramados;
        private int _turnosAceptados;
        private int _turnosDesaprovechados;

        public EstadisticasDoctorWindow(Profesional doctor)
        {
            InitializeComponent();
            _doctor = doctor;
            _apiService = new ApiService();
            CargarDatosDoctor();
            _ = CargarEstadisticas();
        }

        private void CargarDatosDoctor()
        {
            DoctorNombre.Text = _doctor.NombreCompleto;
            DoctorTelefono.Text = "No disponible"; // Agregar si tienes este campo en el modelo
            DoctorEmail.Text = "No disponible"; // Agregar si tienes este campo en el modelo
            DoctorEspecialidad.Text = _doctor.Especialidad?.Nombre ?? "No especificada";
        }

        private async Task CargarEstadisticas()
        {
            try
            {
                // Obtener todos los turnos del doctor
                _todosTurnos = await _apiService.ObtenerTurnosPorDoctor(_doctor.DoctorId);
                
                // Calcular estadísticas
                CalcularEstadisticas();
                
                // Actualizar UI
                ActualizarInterfaz();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalcularEstadisticas()
        {
            if (_todosTurnos == null) return;

            _turnosAtendidos = _todosTurnos.Count(t => t.Estado == EstadoTurno.atendido);
            _turnosCancelados = _todosTurnos.Count(t => t.Estado == EstadoTurno.cancelado);
            _turnosRechazados = _todosTurnos.Count(t => t.Estado == EstadoTurno.rechazado);
            _turnosReprogramados = _todosTurnos.Count(t => t.Estado == EstadoTurno.pendiente); // Asumiendo que reprogramados están como pendientes
            _turnosAceptados = _todosTurnos.Count(t => t.Estado == EstadoTurno.aceptado);
            _turnosDesaprovechados = _todosTurnos.Count(t => t.Estado == EstadoTurno.desaprovechado);
        }

        private void ActualizarInterfaz()
        {
            TurnosAtendidos.Text = _turnosAtendidos.ToString();
            TurnosCancelados.Text = _turnosCancelados.ToString();
            TurnosRechazados.Text = _turnosRechazados.ToString();
            TurnosReprogramados.Text = _turnosReprogramados.ToString();
            TurnosAceptados.Text = _turnosAceptados.ToString();
            TurnosDesaprovechados.Text = _turnosDesaprovechados.ToString();
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                    await GenerarPDF(saveFileDialog.FileName);
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

        private Task GenerarPDF(string rutaArchivo)
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
                
                document.Add(new Paragraph($"Nombre: {_doctor.NombreCompleto}", normalFont) { SpacingAfter = 5f });
                document.Add(new Paragraph($"Teléfono: {DoctorTelefono.Text}", normalFont) { SpacingAfter = 5f });
                document.Add(new Paragraph($"Email: {DoctorEmail.Text}", normalFont) { SpacingAfter = 5f });
                document.Add(new Paragraph($"Especialidad: {_doctor.Especialidad?.Nombre ?? "No especificada"}", normalFont) { SpacingAfter = 20f });

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
                AgregarFilaEstadistica(table, "Turnos Atendidos", _turnosAtendidos, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Cancelados", _turnosCancelados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Rechazados", _turnosRechazados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Reprogramados", _turnosReprogramados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Aceptados", _turnosAceptados, normalFont, statFont);
                AgregarFilaEstadistica(table, "Turnos Desaprovechados", _turnosDesaprovechados, normalFont, statFont);

                document.Add(table);

                // Total de turnos
                int totalTurnos = _turnosAtendidos + _turnosCancelados + _turnosRechazados + 
                                 _turnosReprogramados + _turnosAceptados + _turnosDesaprovechados;
                
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
    }
}
