using System;
using Newtonsoft.Json;

namespace SaludTotal.Services;

public class ResultadoApi
{
    public bool Success { get; set; }
    [JsonProperty("mensaje")]
    public string? Mensaje { get; set; }
    [JsonProperty("detalle")]
    public string? Detalle { get; set; }
    [JsonProperty("turno")]
    public object? Turno { get; set; }
    [JsonProperty("errores")]
    public Dictionary<string, string[]>? Errores { get; set; }
}

public class ErroresFechaYHora
{
    public string[]? Fecha { get; set; }
    public string[]? Hora { get; set; }
}
