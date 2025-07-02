using Newtonsoft.Json;
using SaludTotal.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
        /// Obtiene la lista completa de turnos desde la API.
        /// </summary>
        /// <returns>Una lista de objetos Turno.</returns>
        public async Task<List<Turno>> GetTurnosAsync()
        {
            try
            {
                string url = $"{ApiBaseUrl}/turnos";
                HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<Turno>>(jsonResponse);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error de solicitud HTTP: {e.Message}");
                throw;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error de deserialización JSON: {e.Message}");
                throw;
            }
        }

        public async Task<bool> ConfirmarTurnoAsync(int turnoId)
        {
            try
            {
                string url = $"{ApiBaseUrl}turnos/{turnoId}/confirmar";

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
                string url = $"{ApiBaseUrl}turnos/{turnoId}/cancelar";
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
    }
}