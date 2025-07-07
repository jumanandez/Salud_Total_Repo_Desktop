using System.Windows;

namespace SaludTotal.Desktop.Views
{
    public partial class InformesMenuWindow : Window
    {
        public InformesMenuWindow()
        {
            InitializeComponent();
        }

        private void VolverInicio_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana Dashboard
            var dashboardWindow = new DashboardWindow();
            dashboardWindow.Show();
            
            // Cerrar la ventana actual
            this.Close();
        }

        private void InformesProfesionales_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana de Informes de Profesionales
            var informesProfesionalesWindow = new InformesProfesionalesWindow();
            informesProfesionalesWindow.Show();
            
            // Cerrar la ventana actual
            this.Close();
        }

        private void InformesEmpresa_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar la ventana de Informes de Empresa
            var informesEmpresaWindow = new InformesEmpresaWindow();
            informesEmpresaWindow.Show();
            
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