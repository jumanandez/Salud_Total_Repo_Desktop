using Newtonsoft.Json;

namespace SaludTotal.Models
{
    public class Especialidad
    {
        [JsonProperty("especialidad_id")]
        public int EspecialidadId { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
