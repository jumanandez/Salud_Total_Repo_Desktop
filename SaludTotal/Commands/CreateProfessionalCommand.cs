using SaludTotal.Models;
using SaludTotal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaludTotal.Commands
{
    public class CreateProfessionalCommand : AsyncCommandBase
    {
        private readonly AddProfessionalViewModel _viewModel;

        public CreateProfessionalCommand(AddProfessionalViewModel viewModel)
            : base(_ => viewModel.CanCreate)
        {
            _viewModel = viewModel;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            try
            {
                var professional = new Profesional
                {
                    NombreApellido = _viewModel.Name + " " + _viewModel.LastName,
                    Email = _viewModel.Email,
                    Telefono = _viewModel.Phone,
                    EspecialidadId = _viewModel.SpecialtyId ?? 0,
                };

                await ProfessionalManager.AddProfessional(professional);

                _viewModel.StatusMessage = "Se ha creado el registro correctamente.";
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
        }
    }
}
