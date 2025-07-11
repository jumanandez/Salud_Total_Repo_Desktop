using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using SaludTotal.Models;

namespace SaludTotal.Models
{
    public class Turno
    {
        [JsonProperty("turno_id")]
        public int Id { get; set; }

        [JsonProperty("paciente_id")]
        public int PacienteId { get; set; }

        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }

        [JsonProperty("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonProperty("hora")]
        public string Hora { get; set; } = string.Empty;

        [JsonProperty("estado")]
        public EstadoTurno Estado { get; set; }
        [JsonProperty("solicita_cancelacion")]
        public bool SolicitaCancelacion { get; set; }

        [JsonProperty("paciente")]
        public Paciente? Paciente { get; set; }

        [JsonProperty("doctor")]
        public Profesional? Profesional { get; set; }

        // Propiedad calculada para mostrar fecha y hora juntas
        public string FechaHora 
        { 
            get 
            {
                if (!string.IsNullOrEmpty(Fecha) && !string.IsNullOrEmpty(Hora))
                {
                    return $"{Fecha} {Hora}";
                }
                return string.Empty;
            }
        }
    }
}