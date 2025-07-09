using System;

namespace SaludTotal.Models
{
    public class SolicitudReprogramacion
    {
        public int Id { get; set; }
        public int TurnoId { get; set; }
        public string FechaOriginal { get; set; } = string.Empty;
        public string HoraOriginal { get; set; } = string.Empty;
        public string FechaNueva { get; set; } = string.Empty;
        public string HoraNueva { get; set; } = string.Empty;
        public Paciente? Paciente { get; set; }
        public Profesional? Profesional { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aceptada, Rechazada
        public DateTime FechaSolicitud { get; set; }
        public string? Motivo { get; set; }
    }
}
