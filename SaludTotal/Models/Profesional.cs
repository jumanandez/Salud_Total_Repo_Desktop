using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SaludTotal.Models
{

    public enum Especialidades
    {
        Cardiología = 1,
        Dermatología = 2,
        Endocrinología = 3,
    }

    public class Profesional
    {
        [JsonProperty("doctor_id")]
        public int Id { get; set; }

        [JsonProperty("nombre_apellido")]
        public string NombreApellido { get; set; } = string.Empty;
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
        [JsonProperty("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonProperty("especialidad_id")]
        public int EspecialidadId { get; set; }

        [JsonProperty("especialidad")]
        public Especialidad? Especialidad { get; set; }
        public string NombreCompleto => NombreApellido;

        public string? EspecialidadText
        {
            get
            {
                return Enum.IsDefined(typeof(Especialidades), EspecialidadId)
                    ? ((Especialidades)EspecialidadId).ToString()
                    : null;
            }
        }
    }
}