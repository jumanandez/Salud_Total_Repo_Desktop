using System.Windows;
using SaludTotal.Desktop.ViewModels;

namespace SaludTotal.Desktop.Views
{
    public partial class TestBinding : Window
    {
        private readonly TurnosViewModel _viewModel;

        public TestBinding()
        {
            InitializeComponent();
            _viewModel = new TurnosViewModel();
            this.DataContext = _viewModel;
        }

        private void TestPediatria_Click(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("=== TEST PEDIATRÍA CLICK ===");
            _viewModel.EspecialidadSeleccionada = "Pediatría";
        }

        private void TestTodos_Click(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("=== TEST TODOS CLICK ===");
            _viewModel.EspecialidadSeleccionada = "Todos";
        }
    }
}
