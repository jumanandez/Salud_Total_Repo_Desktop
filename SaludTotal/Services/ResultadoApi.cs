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
}


