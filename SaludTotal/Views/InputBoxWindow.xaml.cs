using System.Windows;

namespace SaludTotal.Desktop.Views
{
    public partial class InputBoxWindow : Window
    {
        public string? UserInput => string.IsNullOrWhiteSpace(InputTextBox.Text) ? null : InputTextBox.Text.Trim();

        public InputBoxWindow(string title, string prompt)
        {
            InitializeComponent();
            this.Title = title;
            PromptTextBlock.Text = prompt;
            InputTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
