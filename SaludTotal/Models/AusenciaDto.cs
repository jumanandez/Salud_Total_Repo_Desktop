using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SaludTotal.Models
{
    public class AusenciaDto
    {
        [JsonProperty("ausencia_id")]
        public int Id { get; set; }
        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }
        [JsonProperty("motivo_id")]
        public int MotivoId { get; set; }
        [JsonProperty("motivo")]
        public MotivoAusencia Motivo { get; set; } = new MotivoAusencia();
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Descripcion { get; set; }
        // Propiedad calculada para mostrar el rango de fechas
        public string RangoFechas => $"{FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}";
        // Propiedad para indicar si la ausencia está activa (hoy está dentro del rango)
        public bool EstaActiva => DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;
    }
    public class MotivoAusencia
    {
        [JsonProperty("motivo_id")]
        public int Id { get; set; }
        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
        [JsonProperty("activo")]
        public bool Activo { get; set; } = true;
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
