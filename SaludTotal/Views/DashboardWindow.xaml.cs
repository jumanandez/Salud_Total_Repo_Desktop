using System.Windows;

namespace SaludTotal.Desktop.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AdministrarTurnos_Click(object sender, RoutedEventArgs e)
        {
            var administracionWindow = new AdministracionTurnosWindow();
            administracionWindow.Show();
        }

        private void GestionPacientes_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar gestión de pacientes
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GestionHorarios_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar gestión de horarios
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void VerInformes_Click(object sender, RoutedEventArgs e)
        {
            // Abrir la ventana de informes existente
            var informesWindow = new InformesMenuWindow();
            informesWindow.Show();
        }
    }
}
