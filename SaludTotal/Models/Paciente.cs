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

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonProperty("apellido")]
        public string Apellido { get; set; } = string.Empty;

        // Propiedad extra para facilitar la visualización en la UI
        // Si viene 'name' del endpoint, lo usa, sino combina nombre y apellido
        public string NombreCompleto => !string.IsNullOrEmpty(Name) ? Name : $"{Nombre} {Apellido}".Trim();
    }
}