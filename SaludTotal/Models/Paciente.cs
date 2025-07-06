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
        [JsonProperty("id")]
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
        public string Name => NombreApellido;
        public string Nombre => NombreApellido.Split(' ').FirstOrDefault() ?? "";
        public string Apellido => string.Join(" ", NombreApellido.Split(' ').Skip(1));
        
        // Propiedad para mostrar información del paciente en el ComboBox
        public string InfoCompleta => $"{NombreApellido} - {Email}{(!string.IsNullOrEmpty(Dni) ? $" - DNI: {Dni}" : "")}";
    }
}