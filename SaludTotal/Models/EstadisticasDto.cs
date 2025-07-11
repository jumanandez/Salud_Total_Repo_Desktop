using Newtonsoft.Json;
using System;

namespace SaludTotal.Models
{
    public class AusenciaRangoDto
    {
        [JsonProperty("fecha_inicio")]
        public DateTime? FechaInicio { get; set; }

        [JsonProperty("fecha_fin")]
        public DateTime? FechaFin { get; set; }
    }
    public class EstadisticasDoctorDto
    {
        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }

        [JsonProperty("nombre")]
        public string NombreDoctor { get; set; } = string.Empty;

        [JsonProperty("especialidad")]
        public string Especialidad { get; set; } = string.Empty;

        [JsonProperty("turnos_atendidos")]
        public int TurnosAtendidos { get; set; }

        [JsonProperty("turnos_cancelados")]
        public int TurnosCancelados { get; set; }

        [JsonProperty("turnos_rechazados")]
        public int TurnosRechazados { get; set; }

        [JsonProperty("turnos_reprogramados")]
        public int TurnosReprogramados { get; set; }

        [JsonProperty("turnos_aceptados")]
        public int TurnosAceptados { get; set; }

        [JsonProperty("turnos_desaprovechados")]
        public int TurnosDesaprovechados { get; set; }
        [JsonProperty("ausencias_anotadas")]
        public int AusenciasAnotadas { get; set; }

        [JsonProperty("dias_ausencia")]
        public int DiasAusencia { get; set; }

        [JsonProperty("ultima_ausencia")]
        public AusenciaRangoDto? UltimaAusencia { get; set; }

        [JsonProperty("total_turnos")]
        public int TotalTurnos { get; set; }

        [JsonProperty("desde")]
        public DateTime? FechaDesde { get; set; }

        [JsonProperty("hasta")]
        public DateTime? FechaHasta { get; set; }
    }

    public class EstadisticasGlobalesDto
    {
        [JsonProperty("totalTurnos")]
        public int TotalTurnos { get; set; }

        [JsonProperty("turnosAtendidos")]
        public int TurnosAtendidos { get; set; }

        [JsonProperty("turnosCancelados")]
        public int TurnosCancelados { get; set; }

        [JsonProperty("turnosRechazados")]
        public int TurnosRechazados { get; set; }

        [JsonProperty("turnosReprogramados")]
        public int TurnosReprogramados { get; set; }

        [JsonProperty("turnosAceptados")]
        public int TurnosAceptados { get; set; }

        [JsonProperty("turnosDesaprovechados")]
        public int TurnosDesaprovechados { get; set; }

        [JsonProperty("totalDoctores")]
        public int TotalDoctores { get; set; }
        [JsonProperty("ausenciasGlobales")]
        public int TotalAusencias { get; set; }
        [JsonProperty("totalPacientes")]
        public int TotalPacientes { get; set; }

        [JsonProperty("desde")]
        public DateTime? FechaDesde { get; set; }

        [JsonProperty("hasta")]
        public DateTime? FechaHasta { get; set; }
    }
}