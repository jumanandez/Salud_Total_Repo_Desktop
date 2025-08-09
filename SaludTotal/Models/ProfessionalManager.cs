using SaludTotal.Desktop.Services;
using SaludTotal.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaludTotal.Models
{
    public class ProfessionalManager
    {
        public static ObservableCollection<Profesional> _databaseProfessionals { get; set; }
        private readonly ApiService _apiService = new();

        public static ObservableCollection<Profesional> GetProfessionals()
        {
            return _databaseProfessionals;
        }

        public static async Task AddProfessional(Profesional profesional)
        {
            ResultadoApi result = await ApiService.AddProfessionalAsync(profesional);
            if (!result.Success)
            {
                throw new Exception(result.Mensaje);
            }
        }

        public async Task<List<Especialidad>> GetSpecializations()
        {
            return await _apiService.GetEspecialidadesAsync();
        }
    }
}
