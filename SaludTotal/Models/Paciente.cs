using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SaludTotal.Models
{
    public class Paciente
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("apellido")]
        public string Apellido { get; set; }

        // Propiedad extra para facilitar la visualización en la UI
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}