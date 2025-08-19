using SaludTotal.Desktop.Views;
using SaludTotal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SaludTotal.Views
{
    /// <summary>
    /// Interaction logic for ProfessionalManagment.xaml
    /// </summary>
    public partial class ProfessionalManagment : Window
    {

        private readonly ProfessionalManagmentViewModel _viewModel;
        public ProfessionalManagment()
        {
            InitializeComponent();
            _viewModel = new ProfessionalManagmentViewModel();
            _viewModel.RequestClose += () => this.Close();
            _viewModel.Window = this;
            DataContext = _viewModel;
        }

        #region WindowManipulationMethods
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {

            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void VolverMenu_Click(object sender, RoutedEventArgs e)
        {
            var dashboardWindow = new DashboardWindow();
            dashboardWindow.Show();
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #endregion

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
