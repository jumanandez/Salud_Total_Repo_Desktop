using System;
using System.Windows;
using SaludTotal.Models;

namespace SaludTotal.Desktop.Views
{
    public partial class DetalleTurnoWindow : Window
    {
        private Turno _turno;
        public bool Confirmado { get; private set; } = false;

        // Constructor nuevo para la gestión de turnos existentes
        public DetalleTurnoWindow(Turno turno)
        {
            InitializeComponent();
            _turno = turno;
            CargarDatosTurno();
        }

        // Constructor compatible con NuevoTurnoWindow (mantiene compatibilidad)
        public DetalleTurnoWindow(Paciente paciente, string especialidad, string doctor, string fecha, string hora)
        {
            InitializeComponent();
            
            // Crear un turno temporal para mostrar la información
            _turno = new Turno
            {
                Paciente = paciente,
                Profesional = new Profesional 
                { 
                    NombreApellido = doctor,
                    Especialidad = new Especialidad { Nombre = especialidad }
                },
                Fecha = fecha,
                Hora = hora
            };
            
            CargarDatosTurno();
        }

        private Especialidad TryParseEspecialidad(string especialidad)
        {
            return new Especialidad { Nombre = especialidad };
        }

        private void CargarDatosTurno()
        {
            if (_turno != null)
            {
                // Información del Paciente
                PacienteNombre.Text = _turno.Paciente?.NombreCompleto ?? "No disponible";
                PacienteTelefono.Text = _turno.Paciente?.Telefono ?? "No disponible";
                PacienteEmail.Text = _turno.Paciente?.Email ?? "No disponible";

                // Información del Doctor
                DoctorNombre.Text = _turno.Profesional?.NombreCompleto ?? "No disponible";
                DoctorEspecialidad.Text = _turno.Profesional?.Especialidad?.ToString() ?? "No disponible";

                // Fecha original
                FechaOriginal.Text = $"{_turno.Fecha} {_turno.Hora}";
            }
        }

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelarTurno_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar lógica para cancelar turno
            MessageBox.Show("Funcionalidad de cancelar turno - Por implementar", "Cancelar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReprogramarTurno_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar lógica para reprogramar turno
            string nuevaFecha = CalendarFecha.SelectedDate?.ToShortDateString() ?? "No seleccionada";
            string nuevaHora = (ComboHora.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "No seleccionada";
            
            MessageBox.Show($"Reprogramar turno:\nFecha: {nuevaFecha}\nHora: {nuevaHora}\n\nFuncionalidad por implementar", 
                           "Reprogramar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AceptarTurno_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar lógica para aceptar/confirmar turno
            MessageBox.Show("Funcionalidad de aceptar turno - Por implementar", "Aceptar Turno", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Métodos para compatibilidad con NuevoTurnoWindow
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
