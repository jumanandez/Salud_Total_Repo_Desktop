using System.Windows;

namespace SaludTotal.Desktop.Views
{
    public partial class InformesMenuWindow : Window
    {
        public InformesMenuWindow()
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
    }
}
