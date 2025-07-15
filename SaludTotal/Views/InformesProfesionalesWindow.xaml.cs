using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace SaludTotal.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para InformesProfesionalesWindow.xaml
    /// </summary>
    public partial class InformesProfesionalesWindow : Window
    {
        private ObservableCollection<Profesional> _todosLosProfesionales = new();
        private ObservableCollection<Profesional> _profesionalesFiltrados = new();
        private readonly ApiService _apiService;

        // --- NUEVO: Generar Informe de Doctores ---
        private ObservableCollection<SaludTotal.Models.EstadisticasDoctorDto> _estadisticasDoctores = new();

        public InformesProfesionalesWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            LoadProfesionalesData();
        }

        private async void LoadProfesionalesData()
        {
            try
            {
                // Cargar todos los doctores con sus especialidades
                await CargarTodosDoctoresConEspecialidadesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los profesionales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Cargar datos de ejemplo como fallback
            }
        }

        private async Task CargarTodosDoctoresConEspecialidadesAsync()
        {
            try
            {
                // Primero obtener todas las especialidades
                var especialidades = await _apiService.GetEspecialidadesAsync();
                var todosLosDoctores = new List<Profesional>();

                Console.WriteLine($"Especialidades encontradas: {especialidades.Count}");

                // Para cada especialidad, obtener sus doctores
                foreach (var especialidad in especialidades)
                {
                    try
                    {
                        var doctoresEspecialidad = await _apiService.GetDoctoresByEspecialidadAsync(especialidad.EspecialidadId);
                        
                        foreach (var doctor in doctoresEspecialidad)
                        {
                            doctor.Especialidad.Nombre = especialidad.Nombre;
                            Console.WriteLine($"Asignando especialidad: '{doctor.Especialidad}' al doctor {doctor.NombreCompleto}");
                            todosLosDoctores.Add(doctor);
                        }
                        
                        Console.WriteLine($"Especialidad '{especialidad.Nombre}': {doctoresEspecialidad.Count} doctores");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar doctores de {especialidad.Nombre}: {ex.Message}");
                    }
                }

                // Actualizar las colecciones
                _todosLosProfesionales.Clear();
                foreach (var doctor in todosLosDoctores)
                {
                    _todosLosProfesionales.Add(doctor);
                }

                _profesionalesFiltrados = new ObservableCollection<Profesional>(_todosLosProfesionales);
                ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
                
                Console.WriteLine($"Total de doctores cargados: {todosLosDoctores.Count}");
                foreach (var doctor in todosLosDoctores.Take(5))
                {
                    Console.WriteLine($"Doctor: {doctor.NombreCompleto}, Especialidad: '{doctor.Especialidad}' (Length: {doctor.Especialidad.Nombre?.Length})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en CargarTodosDoctoresConEspecialidadesAsync: {ex.Message}");
                throw;
            }
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

        private async void FiltrarCardiologia_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Cardiología");
            ActualizarEstadoFiltros("Cardiología");
        }

        private async void FiltrarGinecologia_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Ginecología");
            ActualizarEstadoFiltros("Ginecología");
        }

        private async void FiltrarPediatria_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Pediatría");
            ActualizarEstadoFiltros("Pediatría");
        }

        private async void FiltrarClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            await FiltrarPorEspecialidadAsync("Clínica General");
            ActualizarEstadoFiltros("Clínica General");
        }

        private async void FiltrarTodos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Recargar todos los doctores con sus especialidades
                await CargarTodosDoctoresConEspecialidadesAsync();
                
                // Actualizar el estado visual del filtro
                ActualizarEstadoFiltros("Todos");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar todos los doctores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback a mostrar todos los que ya tenemos cargados
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales)
                {
                    _profesionalesFiltrados.Add(profesional);
                }
                ActualizarEstadoFiltros("Todos");
            }
        }

        private async Task FiltrarPorEspecialidadAsync(string especialidad)
        {
            try
            {
                // Primero obtener las especialidades para conseguir el ID
                var especialidades = await _apiService.GetEspecialidadesAsync();
                
                // Debug: Mostrar especialidades disponibles
                Console.WriteLine($"Especialidades disponibles:");
                foreach (var esp in especialidades)
                {
                    Console.WriteLine($"  - ID: {esp.EspecialidadId}, Nombre: '{esp.Nombre}'");
                }
                
                // Buscar la especialidad de manera flexible (sin tildes, case insensitive)
                var especialidadObj = especialidades.FirstOrDefault(e => 
                    NormalizarTexto(e.Nombre).Equals(NormalizarTexto(especialidad), StringComparison.OrdinalIgnoreCase));
                
                if (especialidadObj != null)
                {
                    Console.WriteLine($"Especialidad encontrada: {especialidadObj.Nombre} (ID: {especialidadObj.EspecialidadId})");
                    
                    // Obtener doctores por especialidad específica
                    var doctores = await _apiService.GetDoctoresByEspecialidadAsync(especialidadObj.EspecialidadId);
                    _profesionalesFiltrados.Clear();
                    
                    foreach (var doctor in doctores)
                    {
                        // Asegurar que la especialidad esté asignada con el nombre original de la API
                        doctor.Especialidad.Nombre = especialidadObj.Nombre;
                        _profesionalesFiltrados.Add(doctor);
                    }
                    
                    Console.WriteLine($"Doctores filtrados: {doctores.Count}");
                }
                else
                {
                    // Si no encontramos la especialidad en la API, intentar filtro local
                    Console.WriteLine($"Especialidad '{especialidad}' no encontrada en API, intentando filtro local");
                    
                    var doctoresFiltrados = _todosLosProfesionales.Where(p => 
                        NormalizarTexto(p.Especialidad.Nombre).Contains(NormalizarTexto(especialidad), StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    _profesionalesFiltrados.Clear();
                    foreach (var doctor in doctoresFiltrados)
                    {
                        _profesionalesFiltrados.Add(doctor);
                    }
                    
                    if (doctoresFiltrados.Count == 0)
                    {
                        MessageBox.Show($"No se encontraron doctores para la especialidad: {especialidad}\n\nEspecialidades disponibles:\n{string.Join(", ", especialidades.Select(e => e.Nombre))}", 
                                      "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar por {especialidad}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback a filtro local
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales.Where(p => 
                    NormalizarTexto(p.Especialidad.Nombre).Contains(NormalizarTexto(especialidad), StringComparison.OrdinalIgnoreCase)))
                {
                    _profesionalesFiltrados.Add(profesional);
                }
            }
        }

        /// <summary>
        /// Normaliza texto removiendo tildes y caracteres especiales para comparaciones flexibles
        /// </summary>
        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            
            return texto
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace("ñ", "n").Replace("Ñ", "N")
                .Trim();
        }

        private void GenerarInforme_Click(object sender, RoutedEventArgs e)
        {
            var modal = new GenerarInformeDoctoresWindow(_apiService);
            modal.Owner = this;
            modal.ShowDialog();
        }

        private void ExportarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Determinar el filtro activo
                string filtroActivo = DeterminarFiltroActivo();
                string nombreArchivo = $"Profesionales_{filtroActivo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                // Configurar el diálogo de guardar archivo
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = nombreArchivo,
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Obtener los datos filtrados actuales
                    var profesionalesParaExportar = _profesionalesFiltrados.ToList();
                    
                    // Generar el PDF
                    GenerarPDFProfesionales(saveFileDialog.FileName, profesionalesParaExportar, filtroActivo);
                    
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

        private string DeterminarFiltroActivo()
        {
            // Verificar qué filtro está activo basándose en los estilos de los botones
            var todosButton = this.FindName("TodosButton") as Button;
            var cardiologiaButton = this.FindName("CardiologiaButton") as Button;
            var ginecologiaButton = this.FindName("GinecologiaButton") as Button;
            var pediatriaButton = this.FindName("PediatriaButton") as Button;
            var clinicaGeneralButton = this.FindName("ClinicaGeneralButton") as Button;

            if (cardiologiaButton?.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Cardiologia";
            if (ginecologiaButton?.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Ginecologia";
            if (pediatriaButton?.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Pediatria";
            if (clinicaGeneralButton?.Style == (Style)FindResource("ActiveFilterButtonStyle"))
                return "Clinica_General";
            
            return "Todos";
        }

        private void GenerarPDFProfesionales(string rutaArchivo, List<Profesional> profesionales, string filtro)
        {
            try
            {
                // Crear el documento PDF
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 50, 50, 50, 50);
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(rutaArchivo, FileMode.Create));
                
                document.Open();

                // Fuentes
                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 18, iTextSharp.text.BaseColor.DARK_GRAY);
                iTextSharp.text.Font headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 14, iTextSharp.text.BaseColor.BLACK);
                iTextSharp.text.Font normalFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10, iTextSharp.text.BaseColor.BLACK);

                // Título del documento
                string tituloFiltro = filtro switch
                {
                    "Cardiologia" => "Doctores de Cardiología",
                    "Ginecologia" => "Doctores de Ginecología", 
                    "Pediatria" => "Doctores de Pediatría",
                    "Clinica_General" => "Doctores de Clínica General",
                    _ => "Todos los Doctores"
                };

                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph(tituloFiltro, titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Información adicional
                document.Add(new iTextSharp.text.Paragraph($"Total de profesionales: {profesionales.Count}", headerFont) { SpacingAfter = 20f });

                // Crear tabla para los profesionales
                iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(4);
                table.WidthPercentage = 100;
                table.SpacingAfter = 20f;
                float[] columnWidths = { 3f, 2f, 3f, 2f };
                table.SetWidths(columnWidths);

                // Headers de la tabla
                AgregarHeaderTabla(table, "Nombre Completo", headerFont);
                AgregarHeaderTabla(table, "Especialidad", headerFont);
                AgregarHeaderTabla(table, "Email", headerFont);
                AgregarHeaderTabla(table, "Teléfono", headerFont);

                // Agregar datos de profesionales
                foreach (var profesional in profesionales)
                {
                    AgregarCeldaTabla(table, profesional.NombreCompleto ?? "", normalFont);
                    AgregarCeldaTabla(table, profesional.Especialidad.Nombre ?? "", normalFont);
                    AgregarCeldaTabla(table, profesional.Email ?? "", normalFont);
                    AgregarCeldaTabla(table, profesional.Telefono ?? "", normalFont);
                }

                document.Add(table);

                // Fecha de generación
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
            iTextSharp.text.pdf.PdfPCell headerCell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(texto, font));
            headerCell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            headerCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            headerCell.Padding = 10f;
            table.AddCell(headerCell);
        }

        private void AgregarCeldaTabla(iTextSharp.text.pdf.PdfPTable table, string texto, iTextSharp.text.Font font)
        {
            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(texto, font));
            cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
            cell.Padding = 8f;
            table.AddCell(cell);
        }

        // Mantener compatibilidad con métodos anteriores
        private void Cardiologia_Click(object sender, RoutedEventArgs e)
        {
            FiltrarCardiologia_Click(sender, e);
        }

        private void Ginecologia_Click(object sender, RoutedEventArgs e)
        {
            FiltrarGinecologia_Click(sender, e);
        }

        private void Pediatria_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPediatria_Click(sender, e);
        }

        private void ClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            FiltrarClinicaGeneral_Click(sender, e);
        }

        private void Siguiente_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar funcionalidad del botón Siguiente
            MessageBox.Show("Funcionalidad del botón Siguiente en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Actualiza el estado visual de los botones de filtro
        /// </summary>
        private void ActualizarEstadoFiltros(string filtroActivo)
        {
            // Resetear todos los botones a su estilo normal
            var todosButton = this.FindName("TodosButton") as Button;
            var cardiologiaButton = this.FindName("CardiologiaButton") as Button;
            var ginecologiaButton = this.FindName("GinecologiaButton") as Button;
            var pediatriaButton = this.FindName("PediatriaButton") as Button;
            var clinicaGeneralButton = this.FindName("ClinicaGeneralButton") as Button;

            // Aplicar estilo normal a todos
            if (todosButton != null) todosButton.Style = (Style)FindResource("FilterButtonStyle");
            if (cardiologiaButton != null) cardiologiaButton.Style = (Style)FindResource("FilterButtonStyle");
            if (ginecologiaButton != null) ginecologiaButton.Style = (Style)FindResource("FilterButtonStyle");
            if (pediatriaButton != null) pediatriaButton.Style = (Style)FindResource("FilterButtonStyle");
            if (clinicaGeneralButton != null) clinicaGeneralButton.Style = (Style)FindResource("FilterButtonStyle");

            // Aplicar estilo activo al filtro seleccionado
            Style activeStyle = (Style)FindResource("ActiveFilterButtonStyle");
            
            switch (filtroActivo)
            {
                case "Todos":
                    if (todosButton != null) todosButton.Style = activeStyle;
                    break;
                case "Cardiología":
                    if (cardiologiaButton != null) cardiologiaButton.Style = activeStyle;
                    break;
                case "Ginecología":
                    if (ginecologiaButton != null) ginecologiaButton.Style = activeStyle;
                    break;
                case "Pediatría":
                    if (pediatriaButton != null) pediatriaButton.Style = activeStyle;
                    break;
                case "Clínica General":
                    if (clinicaGeneralButton != null) clinicaGeneralButton.Style = activeStyle;
                    break;
            }
        }

        // Métodos de búsqueda
        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BuscarProfesionales();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            BuscarProfesionales();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            LimpiarBusqueda();
        }

        private void BuscarProfesionales()
        {
            var searchTextBox = FindName("SearchTextBox") as TextBox;
            string busqueda = searchTextBox?.Text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(busqueda))
            {
                // Si no hay texto de búsqueda, mostrar todos los profesionales
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales)
                {
                    _profesionalesFiltrados.Add(profesional);
                }
                return;
            }

            try
            {
                // Filtrar por nombre completo
                var profesionalesFiltrados = _todosLosProfesionales.Where(p =>
                    NormalizarTexto(p.NombreCompleto).Contains(NormalizarTexto(busqueda), StringComparison.OrdinalIgnoreCase) ||
                    NormalizarTexto(p.Especialidad.Nombre).Contains(NormalizarTexto(busqueda), StringComparison.OrdinalIgnoreCase) ||
                    NormalizarTexto(p.Email).Contains(NormalizarTexto(busqueda), StringComparison.OrdinalIgnoreCase)
                ).ToList();

                _profesionalesFiltrados.Clear();
                foreach (var profesional in profesionalesFiltrados)
                {
                    _profesionalesFiltrados.Add(profesional);
                }

                // Mostrar mensaje si no se encontraron resultados
                if (profesionalesFiltrados.Count == 0)
                {
                    MessageBox.Show($"No se encontraron profesionales que coincidan con '{busqueda}'", 
                                  "Búsqueda", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar profesionales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarBusqueda()
        {
            try
            {
                var searchTextBox = FindName("SearchTextBox") as TextBox;
                if (searchTextBox != null)
                {
                    searchTextBox.Text = "";
                }
                
                // Restaurar la vista completa de profesionales
                _profesionalesFiltrados.Clear();
                foreach (var profesional in _todosLosProfesionales)
                {
                    _profesionalesFiltrados.Add(profesional);
                }
                
                // Actualizar visualmente los filtros (simular "Todos")
                ActualizarEstadoFiltros("Todos");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al limpiar búsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EstadisticasDoctor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar que hay un profesional seleccionado
                if (ProfesionalesDataGrid.SelectedItem is Profesional profesionalSeleccionado)
                {
                    // Convertir DoctorDto a Profesional para la ventana de estadísticas
                    var profesional = new SaludTotal.Models.Profesional
                    {
                        Id = profesionalSeleccionado.Id,
                        NombreApellido = profesionalSeleccionado.NombreCompleto,
                        Especialidad = new SaludTotal.Models.Especialidad 
                        { 
                            Nombre = profesionalSeleccionado.Especialidad.Nombre ?? "No especificada" 
                        }
                    };

                    var estadisticasWindow = new EstadisticasDoctorWindow(profesional);
                    estadisticasWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Por favor, seleccione un profesional para ver sus estadísticas.", 
                        "Seleccionar Profesional", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir estadísticas: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
