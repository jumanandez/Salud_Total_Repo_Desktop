using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaludTotal.Models
{
    public class Paciente
    {
        [JsonProperty("paciente_id")]
        public int Id { get; set; }

        [JsonProperty("nombre_apellido")]
        public string NombreApellido { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonProperty("dni")]
        public string Dni { get; set; } = string.Empty;

        // Propiedades para compatibilidad con el código existente
        public string NombreCompleto => NombreApellido;

        // Propiedad para mostrar información del paciente en el ComboBox
        public string InfoCompleta => $"{NombreApellido} - {Email}{(!string.IsNullOrEmpty(Dni) ? $" - DNI: {Dni}" : "")}";

        public override string ToString()
        {
            return InfoCompleta;
        }
    }
}