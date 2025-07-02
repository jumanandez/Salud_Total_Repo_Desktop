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
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("fecha")]
        public DateTime Fecha { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("paciente")]
        public Paciente Paciente { get; set; }

        [JsonProperty("profesional")]
        public Profesional Profesional { get; set; }
    }
}