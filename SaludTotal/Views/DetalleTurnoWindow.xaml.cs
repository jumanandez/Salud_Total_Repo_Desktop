using System;
using System.Windows;
using SaludTotal.Models;

namespace SaludTotal.Desktop.Views
{
    public partial class DetalleTurnoWindow : Window
    {
        public bool Confirmado { get; private set; } = false;

        public DetalleTurnoWindow(Paciente paciente, string especialidad, string doctor, string fecha, string hora)
        {
            InitializeComponent();
            MessageBox.Show($"Hora recibida en modal: '{hora}'", "Debug");
            PacienteTextBlock.Text = paciente.InfoCompleta;
            EspecialidadTextBlock.Text = especialidad;
            DoctorTextBlock.Text = doctor;
            FechaTextBlock.Text = fecha + " - " + (string.IsNullOrWhiteSpace(hora) ? "-" : hora + " hs");
            HoraTurnoTextBlock.Text = string.IsNullOrWhiteSpace(hora) ? "-" : hora;
        }

        private void Enviar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}
