using System.Windows;

namespace SaludTotal.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para InformesEmpresaWindow.xaml
    /// </summary>
    public partial class InformesEmpresaWindow : Window
    {
        public InformesEmpresaWindow()
        {
            InitializeComponent();
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

        private void VerMesPasado_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar la lógica para cargar y mostrar los datos del mes pasado
            MessageBox.Show("Funcionalidad para ver datos del mes pasado en desarrollo.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}