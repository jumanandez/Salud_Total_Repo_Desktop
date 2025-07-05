using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SaludTotal.Models
{
    public class Profesional
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }

        [JsonProperty("nombre_apellido")]
        public string NombreApellido { get; set; } = string.Empty;

        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonProperty("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [JsonProperty("especialidad")]
        public Especialidad? Especialidad { get; set; }

        // Propiedad extra para facilitar la visualización
        // Si viene 'nombre_apellido' del endpoint, lo usa, sino combina nombre y apellido
        public string NombreCompleto => !string.IsNullOrEmpty(NombreApellido) ? NombreApellido : $"{Nombre} {Apellido}".Trim();
    }
}