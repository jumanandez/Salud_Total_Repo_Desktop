using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SaludTotal.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para InformesProfesionalesWindow.xaml
    /// </summary>
    public partial class InformesProfesionalesWindow : Window
    {
        private ObservableCollection<SampleProfesional> _todosLosProfesionales = new();
        private ObservableCollection<SampleProfesional> _profesionalesFiltrados = new();

        public InformesProfesionalesWindow()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // Datos de ejemplo para mostrar en la tabla
            _todosLosProfesionales = new ObservableCollection<SampleProfesional>
            {
                new SampleProfesional { DoctorId = "DOC001", NombreCompleto = "Dr. Juan Carlos Pérez", Email = "juan.perez@saludtotal.com", Telefono = "+54 11 1234-5678", Especialidad = "Cardiología" },
                new SampleProfesional { DoctorId = "DOC002", NombreCompleto = "Dra. María Elena García", Email = "maria.garcia@saludtotal.com", Telefono = "+54 11 2345-6789", Especialidad = "Ginecología" },
                new SampleProfesional { DoctorId = "DOC003", NombreCompleto = "Dr. Roberto Martínez", Email = "roberto.martinez@saludtotal.com", Telefono = "+54 11 3456-7890", Especialidad = "Pediatría" },
                new SampleProfesional { DoctorId = "DOC004", NombreCompleto = "Dra. Ana Sofía López", Email = "ana.lopez@saludtotal.com", Telefono = "+54 11 4567-8901", Especialidad = "Clínica General" },
                new SampleProfesional { DoctorId = "DOC005", NombreCompleto = "Dr. Carlos Eduardo Ruiz", Email = "carlos.ruiz@saludtotal.com", Telefono = "+54 11 5678-9012", Especialidad = "Cardiología" },
                new SampleProfesional { DoctorId = "DOC006", NombreCompleto = "Dra. Laura Patricia Mendoza", Email = "laura.mendoza@saludtotal.com", Telefono = "+54 11 6789-0123", Especialidad = "Ginecología" },
                new SampleProfesional { DoctorId = "DOC007", NombreCompleto = "Dr. Fernando Andrés Silva", Email = "fernando.silva@saludtotal.com", Telefono = "+54 11 7890-1234", Especialidad = "Pediatría" },
                new SampleProfesional { DoctorId = "DOC008", NombreCompleto = "Dra. Isabel Cristina Torres", Email = "isabel.torres@saludtotal.com", Telefono = "+54 11 8901-2345", Especialidad = "Clínica General" }
            };

            _profesionalesFiltrados = new ObservableCollection<SampleProfesional>(_todosLosProfesionales);
            ProfesionalesDataGrid.ItemsSource = _profesionalesFiltrados;
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

        private void FiltrarCardiologia_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPorEspecialidad("Cardiología");
        }

        private void FiltrarGinecologia_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPorEspecialidad("Ginecología");
        }

        private void FiltrarPediatria_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPorEspecialidad("Pediatría");
        }

        private void FiltrarClinicaGeneral_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPorEspecialidad("Clínica General");
        }

        private void FiltrarTodos_Click(object sender, RoutedEventArgs e)
        {
            _profesionalesFiltrados.Clear();
            foreach (var profesional in _todosLosProfesionales)
            {
                _profesionalesFiltrados.Add(profesional);
            }
        }

        private void FiltrarPorEspecialidad(string especialidad)
        {
            _profesionalesFiltrados.Clear();
            foreach (var profesional in _todosLosProfesionales.Where(p => p.Especialidad == especialidad))
            {
                _profesionalesFiltrados.Add(profesional);
            }
        }

        private void GenerarInforme_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar generación de informe
            MessageBox.Show("Generando informe de profesionales - Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportarDatos_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar exportación de datos
            MessageBox.Show("Exportando datos - Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }

    // Clase temporal para datos de ejemplo
    public class SampleProfesional
    {
        public string DoctorId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
    }
}
