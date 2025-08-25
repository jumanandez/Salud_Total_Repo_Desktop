using SaludTotal.Views;
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
            var administracionWindow = new ShiftsManagment();
            administracionWindow.Show();
            
            // Cerrar la ventana actual del Dashboard
            this.Close();
        }

        private void GestionProfesionales_Click(object sender, RoutedEventArgs e)
        {
            var gestionProfesionalesWindow = new ProfessionalManagment();
            gestionProfesionalesWindow.Show();
            
            // Cerrar la ventana actual del Dashboard
            this.Close();
        }

        private void VerInformes_Click(object sender, RoutedEventArgs e)
        {
            // Abrir la ventana de informes existente
            var informesWindow = new InformesMenuWindow();
            informesWindow.Show();
        }
    }
}
