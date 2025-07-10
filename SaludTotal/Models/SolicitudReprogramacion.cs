using System;
using Newtonsoft.Json;

namespace SaludTotal.Models
{
    public class SolicitudReprogramacion
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("turno_id")]
        public int TurnoId { get; set; }

        [JsonProperty("fecha")]
        public string Fecha { get; set; } = string.Empty;
        [JsonProperty("hora")]
        public string Hora { get; set; } = string.Empty;

        [JsonProperty("estado")]
        public EstadoSolicitud Estado { get; set; }
        [JsonProperty("turno")]
        public Turno? Turno { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
