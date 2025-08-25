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
    /// Interaction logic for ShiftsManagment.xaml
    /// </summary>
    public partial class ShiftsManagment : Window
    {

        private readonly ManagementViewModel _viewModel;

        public ShiftsManagment()
        {
            InitializeComponent();
            _viewModel = new();
            _viewModel.RequestClose += () => this.Close();
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
