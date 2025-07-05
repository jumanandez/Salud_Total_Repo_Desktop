using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SaludTotal.Desktop.ViewModels;
using System.Windows;

namespace SaludTotal.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly TurnosViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new TurnosViewModel();
            // Aquí establecemos la conexión entre la Vista y el ViewModel.
            this.DataContext = _viewModel;
        }
        // Añadir estos métodos dentro de la clase MainWindow
        private async void ConfirmarTurno_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ConfirmarTurno();
        }

        private async void CancelarTurno_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.CancelarTurno();
        }

        // Métodos para controles de ventana
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
