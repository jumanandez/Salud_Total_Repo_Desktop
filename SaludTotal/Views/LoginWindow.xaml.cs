using System.Windows;
using System.Windows.Input;

namespace SaludTotal.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private const string CLAVE_CORRECTA = "saludtotal123";

        public LoginWindow()
        {
            InitializeComponent();
            ClavePasswordBox.Focus();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void IngresarButton_Click(object sender, RoutedEventArgs e)
        {
            ValidarLogin();
        }

        private void ClavePasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidarLogin();
            }
        }

        private void ValidarLogin()
        {
            string claveIngresada = ClavePasswordBox.Password;

            if (string.IsNullOrEmpty(claveIngresada))
            {
                ErrorMessage.Text = "Por favor, ingrese la clave de acceso";
                ClavePasswordBox.Focus();
                return;
            }

            if (claveIngresada == CLAVE_CORRECTA)
            {
                // Login exitoso - abrir Dashboard
                var dashboardWindow = new DashboardWindow();
                dashboardWindow.Show();
                this.Close();
            }
            else
            {
                // Clave incorrecta
                ErrorMessage.Text = "Clave de acceso incorrecta";
                ClavePasswordBox.Clear();
                ClavePasswordBox.Focus();
            }
        }
    }
}
