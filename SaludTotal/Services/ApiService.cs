using Newtonsoft.Json;
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
        public ApiService()
        {
            // Configuración inicial del HttpClient, si es necesaria.
            // Por ejemplo, si tuvieras que añadir un token de autenticación:
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tu_token_aqui");
        }

        // --- MÉTODOS PARA INTERACTUAR CON LA API ---

        /// <summary>
        /// Obtiene la lista de turnos desde la API, permitiendo filtrar por especialidad, fecha, doctor y paciente.
        /// </summary>
        /// <param name="especialidad">Nombre de la especialidad</param>
        /// <param name="fecha">Fecha del turno</param>
        /// <param name="doctor">Nombre del doctor</param>
        /// <param name="paciente">Nombre del paciente</param>
        /// <returns>Una lista de objetos Turno.</returns>
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


        /// <summary>
        /// Crea un nuevo turno en la API.
        /// </summary>
        /// <param name="nuevoTurno">Objeto con los datos del nuevo turno.</param>
        /// <returns>El turno creado con todos sus datos.</returns>
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

        /// <summary>
        /// Obtiene los datos necesarios para el formulario de nuevo turno (especialidades, doctores, horarios).
        /// </summary>
        /// <param name="doctorId">ID del doctor para obtener horarios disponibles (opcional).</param>
        /// <param name="fecha">Fecha para obtener horarios disponibles (opcional).</param>
        /// <returns>Datos del formulario incluyendo especialidades, doctores y horarios disponibles.</returns>
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
                        Console.WriteLine($"Datos obtenidos: {datos?.Especialidades?.Count ?? 0} especialidades, {datos?.DoctoresPorEspecialidad?.Count ?? 0} grupos de doctores");
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

        /// <summary>
        /// Obtiene todos los pacientes desde la API.
        /// </summary>
        /// <returns>Una lista de objetos Paciente.</returns>
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

        /// <summary>
        /// Busca pacientes por nombre, email o DNI.
        /// </summary>
        /// <param name="query">Término de búsqueda para nombre, email o DNI</param>
        /// <returns>Una lista de objetos Paciente que coinciden con la búsqueda</returns>
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

        /// <summary>
        /// Obtiene los horarios disponibles para un doctor y una fecha.
        /// </summary>
        /// <param name="doctorId">ID del doctor</param>
        /// <param name="fecha">Fecha en formato YYYY-MM-DD</param>
        /// <returns>Lista de strings con los horarios disponibles</returns>
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

        /// <summary>
        /// Obtiene solo las especialidades y los doctores agrupados por especialidad.
        /// </summary>
        /// <returns>Datos con especialidades y doctores por especialidad.</returns>
        public async Task<(List<EspecialidadDto> Especialidades, List<DoctoresPorEspecialidadDto> DoctoresPorEspecialidad)> GetEspecialidadesYDoctoresAsync()
        {
            string url = $"{ApiTurnosUrl}/especialidades";
            Console.WriteLine($"Obteniendo especialidades y doctores desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var token = Newtonsoft.Json.Linq.JToken.Parse(responseContent);
            if (token is Newtonsoft.Json.Linq.JObject obj)
            {
                var successToken = obj["success"];
                var dataToken = obj["data"];
                if (successToken != null && successToken.Type != Newtonsoft.Json.Linq.JTokenType.Null && successToken.ToObject<bool>() && dataToken != null)
                {
                    var especialidades = dataToken["especialidades"]?.ToObject<List<EspecialidadDto>>() ?? new List<EspecialidadDto>();
                    var doctoresPorEspecialidad = dataToken["doctores_por_especialidad"]?.ToObject<List<DoctoresPorEspecialidadDto>>() ?? new List<DoctoresPorEspecialidadDto>();
                    return (especialidades, doctoresPorEspecialidad);
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
            else
            {
                throw new Exception("Respuesta inesperada del backend");
            }
        }

        /// <summary>
        /// Obtiene la lista de especialidades desde la API.
        /// </summary>
        /// <returns>Lista de especialidades.</returns>
        public async Task<List<EspecialidadDto>> GetEspecialidadesAsync()
        {
            string url = $"{ApiProfesionalesUrl}/especialidades";
            Console.WriteLine($"Obteniendo especialidades desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
            if (obj["especialidades"] != null)
            {
                var especialidades = obj["especialidades"].ToObject<List<EspecialidadDto>>() ?? new List<EspecialidadDto>();
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

        /// <summary>
        /// Obtiene la lista de doctores para una especialidad específica.
        /// </summary>
        /// <param name="especialidadId">ID de la especialidad</param>
        /// <returns>Lista de doctores para la especialidad.</returns>
        public async Task<List<DoctorDto>> GetDoctoresByEspecialidadAsync(int especialidadId)
        {
            string url = $"{ApiProfesionalesUrl}/especialidades/{especialidadId}/doctores";
            Console.WriteLine($"Obteniendo doctores para especialidad {especialidadId} desde: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
            if (obj["doctores_by_especialidad"] != null)
            {
                var doctores = obj["doctores_by_especialidad"].ToObject<List<DoctorDto>>() ?? new List<DoctorDto>();
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

        /// <summary>
        /// Obtiene los horarios laborales de un doctor.
        /// </summary>
        /// <param name="doctorId">ID del doctor</param>
        /// <returns>Lista de horarios laborales.</returns>
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

        /// <summary>
        /// Obtiene los slots de turnos disponibles para un doctor y una fecha.
        /// </summary>
        /// <param name="doctorId">ID del doctor</param>
        /// <param name="fecha">Fecha en formato YYYY-MM-DD</param>
        /// <returns>Lista de slots disponibles.</returns>
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

        /// <summary>
        /// Obtiene todos los doctores desde la API con sus especialidades.
        /// </summary>
        /// <returns>Lista de todos los doctores con especialidades.</returns>
        public async Task<List<DoctorDto>> GetTodosDoctoresAsync()
        {
            try
            {
                string url = $"{ApiProfesionalesUrl}/";
                Console.WriteLine($"Obteniendo todos los doctores desde: {url}");
                HttpResponseMessage response = await client.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {responseContent}");
                response.EnsureSuccessStatusCode();

                var obj = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                if (obj["doctores"] != null)
                {
                    var doctores = obj["doctores"]?.ToObject<List<DoctorDto>>() ?? new List<DoctorDto>();
                    Console.WriteLine($"Doctores obtenidos: {doctores.Count}");

                    // Debug: Mostrar estructura de los primeros doctores
                    for (int i = 0; i < Math.Min(3, doctores.Count); i++)
                    {
                        var doctor = doctores[i];
                        Console.WriteLine($"Doctor {i + 1}: ID={doctor.Id}, Nombre='{doctor.NombreCompletoCalculado}', Especialidad='{doctor.Especialidad}', Email='{doctor.Email}'");
                    }

                    // Si los doctores no tienen especialidad, intentar obtenerla
                    if (doctores.Any() && string.IsNullOrEmpty(doctores.First().Especialidad))
                    {
                        Console.WriteLine("Los doctores no tienen especialidad asignada, intentando obtenerla...");
                        await AsignarEspecialidadesADoctoresAsync(doctores);
                    }

                    return doctores;
                }
                else if (obj["mensaje"] != null)
                {
                    throw new Exception($"Mensaje del backend: {obj["mensaje"]}");
                }
                else
                {
                    throw new Exception("Respuesta inesperada del backend al obtener todos los doctores");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP al obtener todos los doctores: {e.Message}");
                throw new Exception($"Error de conexión: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON al obtener todos los doctores: {e.Message}");
                throw new Exception($"Error en el formato de respuesta: {e.Message}");
            }
        }

        /// <summary>
        /// Asigna especialidades a doctores que no las tienen.
        /// </summary>
        private async Task AsignarEspecialidadesADoctoresAsync(List<DoctorDto> doctores)
        {
            try
            {
                // Obtener especialidades y doctores por especialidad
                var (especialidades, doctoresPorEspecialidad) = await GetEspecialidadesYDoctoresAsync();

                // Crear un diccionario para mapear doctor ID a especialidad
                var doctorEspecialidadMap = new Dictionary<int, string>();

                foreach (var grupo in doctoresPorEspecialidad)
                {
                    foreach (var doctorEsp in grupo.Doctores)
                    {
                        if (!doctorEspecialidadMap.ContainsKey(doctorEsp.Id))
                        {
                            doctorEspecialidadMap[doctorEsp.Id] = grupo.EspecialidadNombre;
                        }
                    }
                }

                // Asignar especialidades a los doctores
                foreach (var doctor in doctores)
                {
                    if (doctorEspecialidadMap.ContainsKey(doctor.Id))
                    {
                        doctor.Especialidad = doctorEspecialidadMap[doctor.Id];
                        Console.WriteLine($"Asignada especialidad '{doctor.Especialidad}' al doctor {doctor.NombreCompletoCalculado}");
                    }
                    else
                    {
                        doctor.Especialidad = "Sin especialidad";
                    }
                }

                Console.WriteLine($"Especialidades asignadas a {doctores.Count} doctores");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al asignar especialidades: {ex.Message}");
                // Asignar una especialidad por defecto
                foreach (var doctor in doctores)
                {
                    if (string.IsNullOrEmpty(doctor.Especialidad))
                    {
                        doctor.Especialidad = "General";
                    }
                }
            }
        }

        public async Task<string> TestConexionAsync()
        {
            try
            {
                string url = $"{ApiTurnosUrl}/test";
                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return $"✅ Conexión exitosa: {jsonResponse}";
            }
            catch (HttpRequestException e)
            {
                return $"❌ Error de conexión HTTP: {e.Message}";
            }
            catch (Exception e)
            {
                return $"❌ Error general: {e.Message}";
            }
        }

        public async Task<string> TestDeserializacionAsync()
        {
            try
            {
                string url = $"{ApiTurnosUrl}/";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var turnos = JsonConvert.DeserializeObject<List<Turno>>(jsonResponse) ?? new List<Turno>();

                string resultado = $"✅ Se deserializaron {turnos.Count} turnos correctamente:\n";

                // Mostrar solo los primeros 3 turnos para debug
                int maxTurnos = Math.Min(3, turnos.Count);
                for (int i = 0; i < maxTurnos; i++)
                {
                    var turno = turnos[i];
                    resultado += $"- Turno {turno.Id}: Paciente='{turno.Paciente?.NombreCompleto ?? "NULL"}', Profesional='{turno.Profesional?.NombreCompleto ?? "NULL"}'\n";
                }

                return resultado;
            }
            catch (Exception e)
            {
                return $"❌ Error en deserialización: {e.Message}";
            }
        }


        /// <summary>
        /// Obtiene todos los turnos de un doctor específico para calcular estadísticas.
        /// </summary>
        /// <param name="doctorId">ID del doctor</param>
        /// <returns>Lista de turnos del doctor</returns>
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

        // --- MÉTODOS PARA ESTADÍSTICAS ---

        /// <summary>
        /// Obtiene las estadísticas de un doctor específico
        /// </summary>
        /// <param name="doctorId">ID del doctor</param>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <returns>Estadísticas del doctor</returns>
        public async Task<EstadisticasDoctorDto> GetEstadisticasDoctorAsync(int doctorId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (fechaDesde.HasValue)
                    queryParams.Add($"fecha_desde={fechaDesde.Value:yyyy-MM-dd}");
                if (fechaHasta.HasValue)
                    queryParams.Add($"fecha_hasta={fechaHasta.Value:yyyy-MM-dd}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{ApiBaseUrl}/estadisticas/doctor/{doctorId}{queryString}";

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var estadisticas = JsonConvert.DeserializeObject<EstadisticasDoctorDto>(jsonResponse);
                    return estadisticas ?? new EstadisticasDoctorDto();
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

        /// <summary>
        /// Obtiene las estadísticas globales del sistema
        /// </summary>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <returns>Estadísticas globales</returns>
        public async Task<EstadisticasGlobalesDto> GetEstadisticasGlobalesAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (fechaDesde.HasValue)
                    queryParams.Add($"fecha_desde={fechaDesde.Value:yyyy-MM-dd}");
                if (fechaHasta.HasValue)
                    queryParams.Add($"fecha_hasta={fechaHasta.Value:yyyy-MM-dd}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{ApiBaseUrl}/estadisticas/globales{queryString}";

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

        /// <summary>
        /// Obtiene las estadísticas de todos los doctores
        /// </summary>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <returns>Lista de estadísticas por doctor</returns>
        public async Task<List<EstadisticasDoctorDto>> GetEstadisticasTodosDoctoresAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (fechaDesde.HasValue)
                    queryParams.Add($"fecha_desde={fechaDesde.Value:yyyy-MM-dd}");
                if (fechaHasta.HasValue)
                    queryParams.Add($"fecha_hasta={fechaHasta.Value:yyyy-MM-dd}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string url = $"{ApiBaseUrl}/estadisticas/doctores{queryString}";

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var estadisticas = JsonConvert.DeserializeObject<List<EstadisticasDoctorDto>>(jsonResponse);
                    return estadisticas ?? new List<EstadisticasDoctorDto>();
                }
                else
                {
                    Console.WriteLine($"Error al obtener estadísticas de todos los doctores: {response.StatusCode}");
                    return new List<EstadisticasDoctorDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en GetEstadisticasTodosDoctoresAsync: {ex.Message}");
                return new List<EstadisticasDoctorDto>();
            }
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
            public DoctorDto? Doctor { get; set; }
        }

    }

    /// <summary>
    /// Clase para enviar datos de nuevo turno a la API.
    /// </summary>
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

    /// <summary>
    /// Clase para recibir datos del formulario desde la API.
    /// </summary>
    public class DatosFormularioResponse
    {
        [JsonProperty("especialidades")]
        public List<EspecialidadDto> Especialidades { get; set; } = new List<EspecialidadDto>();

        [JsonProperty("doctores_por_especialidad")]
        public List<DoctoresPorEspecialidadDto> DoctoresPorEspecialidad { get; set; } = new List<DoctoresPorEspecialidadDto>();

        [JsonProperty("horarios_disponibles")]
        public List<HorarioDisponibleDto> HorariosDisponibles { get; set; } = new List<HorarioDisponibleDto>();
    }

    public class EspecialidadDto
    {
        [JsonProperty("especialidad_id")]
        public int Id { get; set; }

        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class DoctoresPorEspecialidadDto
    {
        [JsonProperty("especialidad_id")]
        public int EspecialidadId { get; set; }

        [JsonProperty("especialidad_nombre")]
        public string EspecialidadNombre { get; set; } = string.Empty;

        [JsonProperty("doctores")]
        public List<DoctorDto> Doctores { get; set; } = new List<DoctorDto>();
    }

    public class DoctorDto
    {
        [JsonProperty("doctor_id")]
        public int Id { get; set; }

        [JsonProperty("nombre_apellido")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [JsonProperty("especialidad")]
        public string Especialidad { get; set; } = string.Empty;

        // Para compatibilidad con el formato de API de todos los doctores
        [JsonProperty("nombre")]
        public string? Nombre { get; set; }

        [JsonProperty("apellido")]
        public string? Apellido { get; set; }

        // Propiedad calculada para mostrar el ID como string
        public string DoctorId => $"DOC{Id:D3}";

        // Propiedad calculada para el nombre completo si viene separado
        public string NombreCompletoCalculado 
        { 
            get 
            {
                if (!string.IsNullOrEmpty(NombreCompleto))
                    return NombreCompleto;
                
                if (!string.IsNullOrEmpty(Nombre) && !string.IsNullOrEmpty(Apellido))
                    return $"Dr(a). {Nombre} {Apellido}";
                
                return "N/A";
            }
        }
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

    public class EstadisticasDoctorDto
    {
        [JsonProperty("doctor_id")]
        public int DoctorId { get; set; }

        [JsonProperty("nombre_doctor")]
        public string NombreDoctor { get; set; } = string.Empty;

        [JsonProperty("total_turnos")]
        public int TotalTurnos { get; set; }

        [JsonProperty("turnos_aceptados")]
        public int TurnosAceptados { get; set; }

        [JsonProperty("turnos_rechazados")]
        public int TurnosRechazados { get; set; }

        [JsonProperty("turnos_cancelados")]
        public int TurnosCancelados { get; set; }

        [JsonProperty("especialidad")]
        public string Especialidad { get; set; } = string.Empty;
    }

    public class EstadisticasGlobalesDto
    {
        [JsonProperty("total_doctores")]
        public int TotalDoctores { get; set; }

        [JsonProperty("total_pacientes")]
        public int TotalPacientes { get; set; }

        [JsonProperty("total_turnos")]
        public int TotalTurnos { get; set; }

        [JsonProperty("turnos_aceptados")]
        public int TurnosAceptados { get; set; }

        [JsonProperty("turnos_rechazados")]
        public int TurnosRechazados { get; set; }

        [JsonProperty("turnos_cancelados")]
        public int TurnosCancelados { get; set; }
    }

    public class SolicitudesReprogramacionResponse
    {
        [JsonProperty("solicitudes")]
        public List<SolicitudReprogramacion> Solicitudes { get; set; } = new List<SolicitudReprogramacion>();

        [JsonProperty("mensaje")]
        public string Mensaje { get; set; } = string.Empty;
    }
}