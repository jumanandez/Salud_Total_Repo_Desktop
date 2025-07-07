using Newtonsoft.Json;
using SaludTotal.Models;
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

        // Reemplaza esta URL con la de tu entorno de Laravel Herd.
        private const string ApiBaseUrl = "http://saludtotal.test";

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
        public async Task<List<Turno>> GetTurnosAsync(string? especialidad = null, string? fecha = null, string? doctor = null, string? paciente = null)
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

                string url = $"{ApiBaseUrl}/turnos";
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

        /// <summary>
        /// Método de prueba para verificar la conectividad con el backend
        /// </summary>
        /// <returns>String con información de diagnóstico</returns>
        public async Task<string> TestConexionAsync()
        {
            try
            {
                string url = $"{ApiBaseUrl}/turnos/test";
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

        public async Task<bool> ConfirmarTurnoAsync(int turnoId)
        {
            try
            {
                string url = $"{ApiBaseUrl}/turnos/{turnoId}/confirmar";

                HttpResponseMessage response = await client.PutAsync(url, null);
                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al confirmar turno: {e.Message}");
                return false;
            }
        }
        public async Task<bool> CancelarTurnoAsync(int turnoId)
        {
            try
            {
                string url = $"{ApiBaseUrl}/turnos/{turnoId}/cancelar";
                HttpResponseMessage response = await client.PutAsync(url, null);
                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error al cancelar turno: {e.Message}");
                return false;
            }
        }
        public async Task<string> TestDeserializacionAsync()
        {
            try
            {
                string url = $"{ApiBaseUrl}/turnos";
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
        /// Crea un nuevo turno en la API.
        /// </summary>
        /// <param name="nuevoTurno">Objeto con los datos del nuevo turno.</param>
        /// <returns>El turno creado con todos sus datos.</returns>
        public async Task<Turno> CrearTurnoAsync(NuevoTurnoRequest nuevoTurno)
        {
            try
            {
                string url = $"{ApiBaseUrl}/api/turnos/store-desktop";
                
                // Debug temporal
                Console.WriteLine($"Creando turno en: {url}");
                Console.WriteLine($"Datos del turno: {JsonConvert.SerializeObject(nuevoTurno, Formatting.Indented)}");
                
                HttpResponseMessage response = await client.PostAsJsonAsync(url, nuevoTurno);
                string responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Respuesta del servidor: {responseContent}");
                response.EnsureSuccessStatusCode();

                // Deserializar la respuesta que viene en formato { "success": true, "message": "...", "data": {...} }
                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);
                if (responseObj?.success != true || responseObj?.data == null)
                {
                    throw new Exception($"Error en la respuesta del servidor: {responseObj?.message ?? "Respuesta inválida"}");
                }
                
                var turnoJson = JsonConvert.SerializeObject(responseObj.data);
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
                string url = $"{ApiBaseUrl}/api/desktop/turnos/disponibles";
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
                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);
                if (responseObj?.success == true && responseObj?.data != null)
                {
                    var datosJson = JsonConvert.SerializeObject(responseObj.data);
                    var datos = JsonConvert.DeserializeObject<DatosFormularioResponse>(datosJson);
                    Console.WriteLine($"Datos obtenidos: {datos?.Especialidades?.Count ?? 0} especialidades, {datos?.DoctoresPorEspecialidad?.Count ?? 0} grupos de doctores");
                    return datos ?? throw new Exception("No se pudieron deserializar los datos del formulario");
                }
                else if (responseObj?.mensaje != null)
                {
                    throw new Exception($"Mensaje del backend: {responseObj.mensaje}");
                }
                else
                {
                    throw new Exception($"Error en la respuesta del servidor: {responseObj?.error ?? "Respuesta inválida"}");
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
                string url = $"{ApiBaseUrl}/api/pacientes";
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

                string url = $"{ApiBaseUrl}/api/desktop/pacientes/buscar?busqueda={Uri.EscapeDataString(query)}";
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

        // ...existing code...
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

        [JsonProperty("especialidad_id")]
        public int EspecialidadId { get; set; }
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
        [JsonProperty("id")]
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
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nombre_completo")]
        public string NombreCompleto { get; set; } = string.Empty;
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
}