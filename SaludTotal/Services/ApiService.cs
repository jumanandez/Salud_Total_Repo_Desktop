﻿using Newtonsoft.Json;
using SaludTotal.Models;
using SaludTotal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; // Necesario para PostAsJsonAsync y PutAsJsonAsync
using System.Threading.Tasks;
namespace SaludTotal.Desktop.Services
{
    public class ApiService
    {
        // HttpClient se debe instanciar UNA SOLA VEZ por aplicación, no en cada llamada.
        // Usar 'static' es una forma simple de lograrlo en este contexto.
        private static readonly HttpClient client = new HttpClient();
        private const string ApiBaseUrl = "http://saludtotal.test/api";
        private const string ApiTurnosUrl = ApiBaseUrl + "/turnos";
        private const string ApiPacientesUrl = ApiBaseUrl + "/pacientes";
        private const string ApiProfesionalesUrl = ApiBaseUrl + "/profesionales";
        private const string ApiEstadisticasUrl = ApiBaseUrl + "/estadisticas";
        public ApiService()
        {
            // Configuración inicial del HttpClient, si es necesaria.
            // Por ejemplo, si tuvieras que añadir un token de autenticación:
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tu_token_aqui");
        }
        public async Task<List<Turno>> GetTurnosAsync(string? especialidad = null, string? fecha = null, string? doctor = null, string? paciente = null, string? estado = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(especialidad))
                    queryParams.Add($"especialidad={Uri.EscapeDataString(especialidad)}");
                if (!string.IsNullOrEmpty(fecha))
                    queryParams.Add($"fecha={Uri.EscapeDataString(fecha)}");
                if (!string.IsNullOrEmpty(doctor))
                    queryParams.Add($"doctor={Uri.EscapeDataString(doctor)}");
                if (!string.IsNullOrEmpty(paciente))
                    queryParams.Add($"paciente={Uri.EscapeDataString(paciente)}");
                if (!string.IsNullOrEmpty(estado))
                    queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
                string url = $"{ApiTurnosUrl}/";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var turnos = JsonConvert.DeserializeObject<List<Turno>>(jsonResponse) ?? new List<Turno>();
                return turnos;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en GetTurnosAsync: {e.Message}");
                throw;
            }
        }
        public async Task<ResultadoApi> AceptarTurnoAsync(int turnoId)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/{turnoId}/aceptar";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                HttpResponseMessage response = await client.SendAsync(request);

                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al aceptar turno: {resultado}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound) //404
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Turno no encontrado",
                            Detalle = resultado?.Detalle ?? "No se encontró el turno con el ID especificado"
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) //400
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Error al aceptar turno",
                            Detalle = resultado?.Detalle ?? "Los datos enviados no son válidos"
                        };
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity) //422
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error de validación",
                            Detalle = resultado?.Detalle ?? "Los datos enviados no cumplen con los requisitos esperados",
                            Errores = resultado?.Errores ?? new Dictionary<string, string[]>()
                        };
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError) //500
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error interno del servidor",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error inesperado en el servidor"
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = response.IsSuccessStatusCode,
                    Mensaje = resultado?.Mensaje ?? "Turno aceptado exitosamente",
                    Turno = resultado?.Turno ?? "Turno aceptado sin detalles adicionales"
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al aceptar turno: {e.Message}");
                return new ResultadoApi
                {
                    Success = false,
                    Mensaje = "Error de conexión al aceptar turno",
                    Detalle = e.Message
                };
            }
        }
        public async Task<ResultadoApi> CancelarTurnoAsync(int turnoId)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/{turnoId}/cancelar";
                HttpResponseMessage response = await client.PatchAsync(url, null);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al cancelar turno: {responseContent}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound) // 404
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Turno No encontrado",
                            Detalle = resultado?.Detalle ?? "Turno No encontrado"
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) // 400
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Error al cancelar turno",
                            Detalle = resultado?.Detalle ?? "Los datos enviados no son válidos"
                        };
                    }
                }

                return new ResultadoApi
                {
                    Success = response.IsSuccessStatusCode,
                    Mensaje = resultado?.Mensaje ?? "Turno cancelado exitosamente"
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al cancelar turno: {e.Message}");
                return new ResultadoApi
                {
                    Success = false,
                    Mensaje = "Error de conexión al cancelar turno",
                    Detalle = e.Message
                };
            }
        }
        public async Task<ResultadoApi> RechazarTurnoAsync(int turnoId, string? mensaje = null)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/{turnoId}/rechazar";
                StringContent? content = null;
                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    var payload = new { mensaje };
                    content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
                }
                HttpResponseMessage response = await client.PatchAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al rechazar turno: {responseContent}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Turno no encontrado",
                            Detalle = resultado?.Detalle ?? "No se encontró el turno con el ID especificado"
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Error al rechazar turno",
                            Detalle = resultado?.Detalle ?? "Los datos enviados no son válidos"
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = response.IsSuccessStatusCode,
                    Mensaje = resultado?.Mensaje ?? "Turno rechazado exitosamente"
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al rechazar turno: {e.Message}");
                return new ResultadoApi
                {
                    Success = false,
                    Mensaje = "Error de conexión al rechazar turno",
                    Detalle = e.Message
                };
            }
        }
        public async Task<ResultadoApi> ReprogramarTurnoAsync(int turnoId, int doctorId, string fecha, string hora)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/{turnoId}/reprogramar";
                var payload = new
                {
                    doctor_id = doctorId,
                    fecha = fecha,
                    hora = hora
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error al reprogramar turno: {responseContent}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Turno no encontrado",
                            Detalle = resultado?.Detalle ?? "No se encontró el turno con el ID especificado"
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return new ResultadoApi
                        {
                            Success = response.IsSuccessStatusCode,
                            Mensaje = resultado?.Mensaje ?? "Error al reprogramar turno",
                            Detalle = resultado?.Detalle ?? "Los datos enviados no son válidos"
                        };
                    }
                    else if ((int)response.StatusCode == 422)
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error de validación",
                            Detalle = responseContent
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = response.IsSuccessStatusCode,
                    Mensaje = resultado?.Mensaje ?? "Turno reprogramado exitosamente"
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al reprogramar turno: {e.Message}");
                return new ResultadoApi
                {
                    Success = false,
                    Mensaje = "Error de conexión al reprogramar turno",
                    Detalle = e.Message
                };
            }
        }
        public async Task<Turno> CrearTurnoAsync(NuevoTurnoRequest nuevoTurno)
        {
            try
            {
                // Construir la URL con los parámetros como query string
                string url = $"{ApiTurnosUrl}/store?paciente_id={nuevoTurno.PacienteId}&doctor_id={nuevoTurno.DoctorId}&fecha={Uri.EscapeDataString(nuevoTurno.Fecha)}&hora={Uri.EscapeDataString(nuevoTurno.Hora)}";

                // Enviar POST con body vacío
                HttpResponseMessage response = await client.PostAsync(url, null);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity) // 422
                {
                    // Errores de validación
                    dynamic errorObj = JsonConvert.DeserializeObject(responseContent);
                    string mensaje = errorObj?.mensaje ?? "Error de validación";
                    string errores = errorObj?.errores != null ? JsonConvert.SerializeObject(errorObj.errores) : "";
                    throw new Exception($"{mensaje}\n{errores}");
                }
                else if ((int)response.StatusCode >= 400)
                {
                    // Otros errores
                    dynamic errorObj = JsonConvert.DeserializeObject(responseContent);
                    string mensaje = errorObj?.mensaje ?? "Error desconocido";
                    string detalle = errorObj?.detalle != null ? errorObj.detalle.ToString() : "";
                    throw new Exception($"{mensaje}\n{detalle}");
                }

                // Éxito: buscar el objeto Turno
                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);
                if (responseObj?.Turno == null)
                {
                    throw new Exception($"Error en la respuesta del servidor: {responseObj?.mensaje ?? "Respuesta inválida"}");
                }
                var turnoJson = JsonConvert.SerializeObject(responseObj.Turno);
                var turno = JsonConvert.DeserializeObject<Turno>(turnoJson);
                Console.WriteLine($"Turno creado exitosamente: ID={turno?.Id}");
                return turno ?? throw new Exception("No se pudo deserializar el turno creado");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al crear turno: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al crear turno: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<DatosFormularioResponse> GetDatosFormularioAsync(int? doctorId = null, string? fecha = null)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/disponibles";
                var queryParams = new List<string>();
                if (doctorId.HasValue)
                    queryParams.Add($"doctor_id={doctorId.Value}");
                if (!string.IsNullOrEmpty(fecha))
                    queryParams.Add($"fecha={Uri.EscapeDataString(fecha)}");
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);
                Console.WriteLine($"Obteniendo datos del formulario desde: {url}");
                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");
                response.EnsureSuccessStatusCode();

                var token = Newtonsoft.Json.Linq.JToken.Parse(responseContent);
                if (token is Newtonsoft.Json.Linq.JObject obj)
                {
                    var successToken = obj["success"];
                    var dataToken = obj["data"];
                    if (successToken != null && successToken.Type != Newtonsoft.Json.Linq.JTokenType.Null && successToken.ToObject<bool>() && dataToken != null)
                    {
                        var datos = dataToken.ToObject<DatosFormularioResponse>();
                        Console.WriteLine($"Datos obtenidos: {datos?.Especialidades?.Count ?? 0} especialidades, {datos?.DoctoresPorEspecialidadDto?.Count ?? 0} grupos de doctores");
                        return datos ?? throw new Exception("No se pudieron deserializar los datos del formulario");
                    }
                    else if (obj["mensaje"] != null)
                    {
                        throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
                    }
                    else
                    {
                        throw new Exception($"Error en la respuesta del servidor: {obj["error"] ?? "Respuesta inválida"}");
                    }
                }
                else if (token is Newtonsoft.Json.Linq.JArray arr)
                {
                    // Si el backend responde con un array directo (legacy)
                    var horarios = arr.ToObject<List<HorarioDisponibleDto>>() ?? new List<HorarioDisponibleDto>();
                    var datos = new DatosFormularioResponse
                    {
                        HorariosDisponibles = horarios
                    };
                    return datos;
                }
                else
                {
                    throw new Exception("Respuesta inesperada del backend");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener datos del formulario: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener datos del formulario: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<List<SaludTotal.Models.Paciente>> GetPacientesAsync()
        {
            try
            {
                string url = $"{ApiPacientesUrl}";
                Console.WriteLine($"Obteniendo pacientes desde: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Respuesta del servidor: {responseContent}");
                response.EnsureSuccessStatusCode();

                // Deserializar la respuesta que viene en formato { "success": true, "data": [...] }
                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);
                if (responseObj?.success != true || responseObj?.data == null)
                {
                    throw new Exception($"Error en la respuesta del servidor: {responseObj?.error ?? "Respuesta inválida"}");
                }

                var pacientesJson = JsonConvert.SerializeObject(responseObj.data);
                var pacientes = JsonConvert.DeserializeObject<List<SaludTotal.Models.Paciente>>(pacientesJson) ?? new List<SaludTotal.Models.Paciente>();

                Console.WriteLine($"Pacientes obtenidos: {pacientes.Count}");
                return pacientes;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener pacientes: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener pacientes: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<List<SaludTotal.Models.Paciente>> BuscarPacientesAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new List<SaludTotal.Models.Paciente>();
                }

                string url = $"{ApiPacientesUrl}/buscar?busqueda={Uri.EscapeDataString(query)}";
                Console.WriteLine($"Buscando pacientes en: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();

                // Intenta deserializar como objeto con success/data
                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);

                if (responseObj?.success == true && responseObj?.data != null)
                {
                    var pacientesJson = JsonConvert.SerializeObject(responseObj.data);
                    var pacientes = JsonConvert.DeserializeObject<List<SaludTotal.Models.Paciente>>(pacientesJson) ?? new List<SaludTotal.Models.Paciente>();
                    Console.WriteLine($"Pacientes encontrados: {pacientes.Count}");
                    return pacientes;
                }
                else if (responseObj?.mensaje != null)
                {
                    Console.WriteLine($"Mensaje del backend: {responseObj.mensaje}");
                    return new List<SaludTotal.Models.Paciente>();
                }
                else
                {
                    throw new Exception($"Error en la respuesta del servidor: {responseObj?.error ?? "Respuesta inválida"}");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al buscar pacientes: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al buscar pacientes: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<List<string>> GetHorariosDisponiblesAsync(int doctorId, string fecha)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/disponibles?doctor_id={doctorId}&fecha={Uri.EscapeDataString(fecha)}";
                Console.WriteLine($"Obteniendo horarios desde: {url}");
                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");
                response.EnsureSuccessStatusCode();

                var token = Newtonsoft.Json.Linq.JToken.Parse(responseContent);
                if (token is Newtonsoft.Json.Linq.JArray arr)
                {
                    // El backend responde con un array de strings
                    var horarios = arr.ToObject<List<string>>() ?? new List<string>();
                    return horarios;
                }
                else if (token is Newtonsoft.Json.Linq.JObject obj && obj["mensaje"] != null)
                {
                    throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
                }
                else
                {
                    throw new Exception("Respuesta inesperada del backend al obtener horarios disponibles");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener horarios: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener horarios: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<List<Especialidad>> GetEspecialidadesAsync()
        {
            string url = $"{ApiProfesionalesUrl}/especialidades";
            Console.WriteLine($"Obteniendo especialidades desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
            if (obj["especialidades"] != null)
            {
                var especialidades = obj["especialidades"].ToObject<List<Especialidad>>() ?? new List<Especialidad>();
                return especialidades;
            }
            else if (obj["mensaje"] != null)
            {
                throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
            }
            else
            {
                throw new Exception("Respuesta inesperada del backend al obtener especialidades");
            }
        }
        public async Task<List<Profesional>> GetDoctoresAsync()
        {
            string url = $"{ApiProfesionalesUrl}/";
            Console.WriteLine($"Obteniendo doctores desde: {url}");
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var data = JsonConvert.DeserializeObject<ResponseDoctoresDto>(responseContent);
                return data.Doctores ?? new List<Profesional>();
            }
            catch (Exception reqEx)
            {
                throw new Exception(reqEx.Message); //FEO FEO FEO
            }
        }
        public class ResponseDoctoresDto
        {
            [JsonProperty("doctores")]
            public List<Profesional>? Doctores { get; set; } = new List<Profesional>();
            [JsonProperty("mensaje")]
            public string? Mensaje { get; set; }
            [JsonProperty("detalle")]
            public string? Detalle { get; set; }
        }
        public async Task<List<Profesional>> GetDoctoresByEspecialidadAsync(int especialidadId)
        {                                                                   
            string url = $"{ApiProfesionalesUrl}/especialidades/{especialidadId}/doctores";
            Console.WriteLine($"Obteniendo doctores para especialidad {especialidadId} desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
            if (obj["doctores_by_especialidad"] != null)
            {
                var doctores = obj["doctores_by_especialidad"].ToObject<List<Profesional>>() ?? new List<Profesional>();
                return doctores;
            }
            else if (obj["mensaje"] != null)
            {
                throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
            }
            else
            {
                throw new Exception("Respuesta inesperada del backend al obtener doctores por especialidad");
            }
        }
        public async Task<List<HorarioLaboralDto>> GetHorariosLaboralesAsync(int doctorId)
        {
            string url = $"{ApiProfesionalesUrl}/{doctorId}/horarios";
            Console.WriteLine($"Obteniendo horarios laborales para doctor {doctorId} desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
            if (obj["horarios_laborales"] != null)
            {
                var horarios = obj["horarios_laborales"].ToObject<List<HorarioLaboralDto>>() ?? new List<HorarioLaboralDto>();
                return horarios;
            }
            else if (obj["mensaje"] != null)
            {
                throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
            }
            else
            {
                throw new Exception("Respuesta inesperada del backend al obtener horarios laborales");
            }
        }
        public async Task<List<SlotTurnoDto>> GetSlotsTurnosDisponiblesAsync(int doctorId, string fecha)
        {
            string url = $"{ApiTurnosUrl}/disponibles?doctor_id={doctorId}&fecha={Uri.EscapeDataString(fecha)}";
            Console.WriteLine($"Obteniendo slots de turnos disponibles para doctor {doctorId} y fecha {fecha} desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var obj = Newtonsoft.Json.Linq.JToken.Parse(responseContent);
            if (obj["slots"] != null)
            {
                if (obj["slots"].Type == Newtonsoft.Json.Linq.JTokenType.Array && obj["slots"].First?.Type == Newtonsoft.Json.Linq.JTokenType.String)
                {
                    // Si es un array de strings, mapear a SlotTurnoDto
                    var horas = obj["slots"].ToObject<List<string>>() ?? new List<string>();
                    return horas.Select(h => new SlotTurnoDto { Hora = h }).ToList();
                }
                else
                {
                    // Si es un array de objetos SlotTurnoDto
                    var slots = obj["slots"].ToObject<List<SlotTurnoDto>>() ?? new List<SlotTurnoDto>();
                    return slots;
                }
            }
            else if (obj["mensaje"] != null)
            {
                throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
            }
            else
            {
                throw new Exception("Respuesta inesperada del backend al obtener slots de turnos disponibles");
            }
        }
        public async Task<List<Turno>> ObtenerTurnosPorDoctor(int doctorId)
        {
            try
            {
                // Usar el método existente pero filtrar por doctor
                var turnos = await GetTurnosAsync(doctor: doctorId.ToString());
                return turnos;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener turnos del doctor {doctorId}: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener turnos del doctor {doctorId}: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<EstadisticasDoctorDto> GetEstadisticasDoctorAsync(int doctorId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (fechaDesde.HasValue)
                    queryParams.Add($"desde={fechaDesde.Value:yyyy-MM-dd}");
                if (fechaHasta.HasValue)
                    queryParams.Add($"hasta={fechaHasta.Value:yyyy-MM-dd}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{ApiEstadisticasUrl}/doctor/{doctorId}{queryString}";

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var estadisticas = JsonConvert.DeserializeObject<ResponseEstadisticasDoctor>(jsonResponse);
                    return estadisticas.Estadisticas ?? new EstadisticasDoctorDto();
                }
                else
                {
                    Console.WriteLine($"Error al obtener estadísticas del doctor: {response.StatusCode}");
                    return new EstadisticasDoctorDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en GetEstadisticasDoctorAsync: {ex.Message}");
                return new EstadisticasDoctorDto();
            }
        }
        public class ResponseEstadisticasDoctor
        {
            [JsonProperty("estadisticas_doctor")]
            public EstadisticasDoctorDto? Estadisticas { get; set; }

            [JsonProperty("mensaje")]
            public string? Mensaje { get; set; }
            
            [JsonProperty("detalle")]
            public string? Detalle { get; set; }
        }
        public async Task<EstadisticasGlobalesDto> GetEstadisticasGlobalesAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (fechaDesde.HasValue)
                    queryParams.Add($"desde={fechaDesde.Value:yyyy-MM-dd}");
                if (fechaHasta.HasValue)
                    queryParams.Add($"hasta={fechaHasta.Value:yyyy-MM-dd}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{ApiEstadisticasUrl}/globales{queryString}";

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var estadisticas = JsonConvert.DeserializeObject<EstadisticasGlobalesDto>(jsonResponse);
                    return estadisticas ?? new EstadisticasGlobalesDto();
                }
                else
                {
                    Console.WriteLine($"Error al obtener estadísticas globales: {response.StatusCode}");
                    return new EstadisticasGlobalesDto();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en GetEstadisticasGlobalesAsync: {ex.Message}");
                return new EstadisticasGlobalesDto();
            }
        }
        public async Task<ResponseEstadisticasDoctores> GetEstadisticasTodosDoctoresAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (fechaDesde.HasValue)
                    queryParams.Add($"desde={fechaDesde.Value:yyyy-MM-dd}");
                if (fechaHasta.HasValue)
                    queryParams.Add($"hasta={fechaHasta.Value:yyyy-MM-dd}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{ApiEstadisticasUrl}/doctores{queryString}";

                HttpResponseMessage response = await client.GetAsync(url);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var estadisticasResponse = JsonConvert.DeserializeObject<ResponseEstadisticasDoctores>(jsonResponse);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        Console.WriteLine("Error interno del servidor al obtener estadísticas de doctores.");
                        return new ResponseEstadisticasDoctores
                        {
                            Mensaje = estadisticasResponse?.Mensaje ?? "Error interno del servidor al obtener estadísticas de doctores.",
                            Detalle = estadisticasResponse?.Detalle ?? "Ocurrió un error al procesar la solicitud de estadísticas de doctores."
                        };
                    }
                    else
                    {
                        Console.WriteLine($"Error al obtener estadísticas de todos los doctores: {response.StatusCode}");
                        return new ResponseEstadisticasDoctores
                        {
                            Mensaje = estadisticasResponse?.Mensaje ?? "Error interno del servidor al obtener estadísticas de doctores.",
                            Detalle = estadisticasResponse?.Detalle ?? "Ocurrió un error al procesar la solicitud de estadísticas de doctores."
                        };
                    }
                }
                return estadisticasResponse ?? new ResponseEstadisticasDoctores
                {
                    EstadisticasDoctorDtos = new List<EstadisticasDoctorDto>(),
                    Desde = fechaDesde,
                    Hasta = fechaHasta,
                    Mensaje = "No se obtuvieron las estadisticas",
                    Detalle = "No hay detalles adicionales."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en GetEstadisticasTodosDoctoresAsync: {ex.Message}");
                return new ResponseEstadisticasDoctores
                {
                    Mensaje = "Error al obtener estadísticas de doctores.",
                    Detalle = ex.Message
                };
            }
        }
        public class ResponseEstadisticasDoctores
        {
            [JsonProperty("estadisticas_doctores")]
            public List<EstadisticasDoctorDto>? EstadisticasDoctorDtos { get; set; }

            [JsonProperty("desde")]
            public DateTime? Desde { get; set; }

            [JsonProperty("hasta")]
            public DateTime? Hasta { get; set; }
            [JsonProperty("mensaje")]
            public string? Mensaje { get; set; }

            [JsonProperty("detalle")]
            public string? Detalle { get; set; }
        }
        public async Task<SolicitudesReprogramacionResponse> GetSolicitudesDeReprogramacion()
        {
            try
            {
                string url = $"{ApiTurnosUrl}/solicitudes-reprogramacion";
                Console.WriteLine($"Obteniendo solicitudes de reprogramación desde: {url}");
                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var result = JsonConvert.DeserializeObject<SolicitudesReprogramacionResponse>(responseContent);
                if (result == null)
                {
                    throw new Exception("Respuesta inesperada del backend al obtener solicitudes de reprogramación.");
                }
                Console.WriteLine($"Solicitudes de reprogramación obtenidas: {result.Solicitudes?.Count ?? 0}");
                return result;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener solicitudes de reprogramación: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener solicitudes de reprogramación: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<ResultadoApi> AceptarSolicitudReprogramacionAsync(int solicitudId)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/solicitudes-reprogramacion/{solicitudId}/aceptar";
                Console.WriteLine($"Aceptando solicitud de reprogramación {solicitudId} desde: {url}");
                HttpResponseMessage response = await client.PatchAsync(url, null);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Solicitud de reprogramación no encontrada.",
                            Detalle = resultado?.Detalle ?? "No se pudo encontrar la solicitud de reprogramación especificada."
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent) //422 Validaciones
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Solicitud de reprogramación inválida.",
                            Detalle = resultado?.Detalle ?? "La solicitud de reprogramación no es válida.",
                            Errores = resultado?.Errores ?? null
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) //500 Error interno
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error interno del servidor.",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud de reprogramación."
                        };
                    }
                    else
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error al aceptar solicitud de reprogramación. Sin Mensaje",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud. Sin Detalles"
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = true,
                    Mensaje = resultado?.Mensaje ?? "Solicitud de reprogramación aceptada correctamente.",
                    Turno = resultado?.Turno ?? "La solicitud de reprogramación se ha procesado exitosamente."
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al aceptar solicitud de reprogramación: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al aceptar solicitud de reprogramación: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<ResultadoApi> RechazarSolicitudReprogramacionAsync(int solicitudId)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/solicitudes-reprogramacion/{solicitudId}/rechazar";
                Console.WriteLine($"Aceptando solicitud de reprogramación {solicitudId} desde: {url}");
                HttpResponseMessage response = await client.PatchAsync(url, null);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Solicitud de reprogramación o Turno no encontrada.",
                            Detalle = resultado?.Detalle ?? "No se pudo encontrar la solicitud de reprogramación o Turno especificado."
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) //500 Error interno
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error interno del servidor.",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud de reprogramación."
                        };
                    }
                    else
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error al rechazar solicitud de reprogramación. Sin Mensaje",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud. Sin Detalles"
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = true,
                    Mensaje = resultado?.Mensaje ?? "Solicitud de reprogramación Rechazada correctamente.",
                    Turno = resultado?.Turno ?? "La solicitud de reprogramación se ha procesado exitosamente."
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al rechazar solicitud de reprogramación: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al rechazar solicitud de reprogramación: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }
        public async Task<ResultadoApi> AceptarSolicitudCancelacionAsync(int turno_id)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/solicitudes-cancelacion/{turno_id}/aceptar";
                Console.WriteLine($"Aceptando solicitud de cancelación {turno_id} desde: {url}");
                HttpResponseMessage response = await client.PatchAsync(url, null);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Solicitud de cancelación no encontrada.",
                            Detalle = resultado?.Detalle ?? "No se pudo encontrar la solicitud de cancelación especificada."
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent) //422 Validaciones
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Solicitud de cancelación inválida.",
                            Detalle = resultado?.Detalle ?? "La solicitud de cancelación no es válida.",
                            Errores = resultado?.Errores ?? null
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) //500 Error interno
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error interno del servidor.",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud de cancelación."
                        };
                    }
                    else
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error al aceptar solicitud de cancelación. Sin Mensaje",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud. Sin Detalles"
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = true,
                    Mensaje = resultado?.Mensaje ?? "Solicitud de cancelación aceptada correctamente.",
                    Turno = resultado?.Turno ?? "La solicitud de cancelación se ha procesado exitosamente."
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al aceptar solicitud de cancelación: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
        }
        public async Task<ResultadoApi> RechazarSolicitudCancelacionAsync(int turno_id)
        {
            try
            {
                string url = $"{ApiTurnosUrl}/solicitudes-cancelacion/{turno_id}/rechazar";
                Console.WriteLine($"Rechazando solicitud de cancelación {turno_id} desde: {url}");
                HttpResponseMessage response = await client.PatchAsync(url, null);
                string responseContent = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Turno no encontrada.",
                            Detalle = resultado?.Detalle ?? "No se pudo encontrar la Turno especificada."
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) //500 Error interno
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error interno del servidor.",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud de cancelación."
                        };
                    }
                    else
                    {
                        return new ResultadoApi
                        {
                            Success = false,
                            Mensaje = resultado?.Mensaje ?? "Error al rechazar solicitud de cancelación. Sin Mensaje",
                            Detalle = resultado?.Detalle ?? "Ocurrió un error al procesar la solicitud. Sin Detalles"
                        };
                    }
                }
                return new ResultadoApi
                {
                    Success = true,
                    Mensaje = resultado?.Mensaje ?? "Solicitud de cancelación rechazada correctamente.",
                    Turno = resultado?.Turno ?? "La solicitud de cancelación se ha procesado exitosamente."
                };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al rechazar solicitud de cancelación: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
        }
        public async Task<SolicitudesCancelacionResponse> GetSolicitudesDeCancelacionAsync()
        {
            try
            {
                string url = $"{ApiTurnosUrl}/solicitudes-cancelacion";
                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                var result = new SolicitudesCancelacionResponse();
                result.Mensaje = obj["mensaje"]?.ToString() ?? string.Empty;
                var solicitudesToken = obj["solicitudes"];
                if (solicitudesToken != null)
                {
                    if (solicitudesToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        result.Solicitudes = solicitudesToken.ToObject<List<SolicitudCancelacion>>() ?? new List<SolicitudCancelacion>();
                    }
                    else if (solicitudesToken.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                    {
                        var unica = solicitudesToken.ToObject<SolicitudCancelacion>();
                        result.Solicitudes = unica != null ? new List<SolicitudCancelacion> { unica } : new List<SolicitudCancelacion>();
                    }
                    else
                    {
                        result.Solicitudes = new List<SolicitudCancelacion>();
                    }
                }
                else
                {
                    result.Solicitudes = new List<SolicitudCancelacion>();
                }
                return result;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener solicitudes de cancelación: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener solicitudes de cancelación: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }

        public static async Task<ResultadoApi> AddProfessionalAsync(Profesional profesional)
        {
            try
            {
                string url = $"{ApiProfesionalesUrl}/";
                Console.WriteLine($"Agregando profesional en: {url}");
                var jsonContent = JsonConvert.SerializeObject(profesional);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                var resultado = JsonConvert.DeserializeObject<ResultadoApi>(responseContent);
                return resultado ?? new ResultadoApi { Success = false, Mensaje = "Error al agregar profesional" };
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al agregar profesional: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al agregar profesional: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }

        public class SolicitudesCancelacionResponse
        {
            [JsonProperty("mensaje")]
            public string? Mensaje { get; set; } = string.Empty;
            [JsonProperty("solicitudes")]
            public List<SolicitudCancelacion>? Solicitudes { get; set; } = new List<SolicitudCancelacion>();


        }
        public class SolicitudCancelacion
        {
            [JsonProperty("turno_id")]
            public int Id { get; set; }
            [JsonProperty("fecha")] 
            public string Fecha { get; set; } = string.Empty;
            [JsonProperty("hora")]
            public string Hora { get; set; } = string.Empty;

            [JsonProperty("estado")]
            public string Estado { get; set; } = string.Empty;


            [JsonProperty("fecha_solicitud_cancelacion")]
            public string FechaSolicitud { get; set; } = string.Empty;

            [JsonProperty("paciente")]
            public Paciente? Paciente { get; set; }

            [JsonProperty("doctor")]
            public Profesional? Doctor { get; set; }
        }
    }
    public class NuevoTurnoRequest
    {
        [JsonProperty("paciente_id")]
        public int PacienteId { get; set; }

        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }

        [JsonProperty("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonProperty("hora")]
        public string Hora { get; set; } = string.Empty;
    }
    public class DatosFormularioResponse
    {
        [JsonProperty("especialidades")]
        public List<Especialidad> Especialidades { get; set; } = new List<Especialidad>();

        [JsonProperty("doctores_por_especialidad")]
        public List<DoctoresPorEspecialidadDto> DoctoresPorEspecialidadDto { get; set; } = new List<DoctoresPorEspecialidadDto>();

        [JsonProperty("horarios_disponibles")]
        public List<HorarioDisponibleDto> HorariosDisponibles { get; set; } = new List<HorarioDisponibleDto>();
    }
    public class DoctoresPorEspecialidadDto
    {
        [JsonProperty("especialidad_id")]
        public int EspecialidadId { get; set; }

        [JsonProperty("especialidad_nombre")]
        public string EspecialidadNombre { get; set; } = string.Empty;

        [JsonProperty("doctores")]
        public List<Profesional> Doctores { get; set; } = new List<Profesional>();
    }
    public class HorarioDisponibleDto
    {
        [JsonProperty("hora")]
        public string Hora { get; set; } = string.Empty;

        [JsonProperty("disponible")]
        public bool Disponible { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; } = string.Empty;
    }
    public class HorarioLaboralDto
    {
        [JsonProperty("disponibilidad_id")]
        public int DisponibilidadId { get; set; }

        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }

        [JsonProperty("dia_semana")]
        public int DiaSemana { get; set; }

        [JsonProperty("hora_inicio")]
        public string HoraInicio { get; set; } = string.Empty;

        [JsonProperty("hora_fin")]
        public string HoraFin { get; set; } = string.Empty;

        [JsonProperty("estado")]
        public int Estado { get; set; }

        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string? UpdatedAt { get; set; }
    }
    public class SlotTurnoDto
    {
        [JsonProperty("hora")]
        public string Hora { get; set; } = string.Empty;
    }
    public class SolicitudesReprogramacionResponse
    {
        [JsonProperty("solicitudes")]
        public List<SolicitudReprogramacion> Solicitudes { get; set; } = new List<SolicitudReprogramacion>();

        [JsonProperty("mensaje")]
        public string Mensaje { get; set; } = string.Empty;
    }
}