using SaludTotal.Desktop.Services;
using SaludTotal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaludTotal.Commands
{
    public class RejectRequestAsyncCommand : AsyncCommandBase
    {
        private readonly RequestsViewModel _viewModel;
        private readonly ApiService _apiService;
        public RejectRequestAsyncCommand(RequestsViewModel viewModel, ApiService apiservice)
    : base(viewModel.CanExecuteRejectRequest!)
        {
            _viewModel = viewModel;
            _apiService = apiservice;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {

            if (parameter is not int)
            {
                _viewModel.ErrorMessage = "No se pudo obtener datos de la solicitud";
                return;
            }

            var list = _viewModel.CurrentlyExecutingIds;
            list.Add((int)parameter!);
            _viewModel.CurrentlyExecutingIds = list;

            try
            {
                await Task.Delay(1000);
                var result = await _apiService.RechazarSolicitudReprogramacionAsync((int)parameter!);

                if (!result.Success)
                {
                    _viewModel.ErrorMessage = $"Error de backend: {result.Mensaje}";
                }
                else
                {
                    _viewModel.StatusMessage = "Se ha actualizado el registro correctamente.";
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
            finally
            {
                _viewModel.ExecuteReload();
                list.Remove((int)parameter);
                _viewModel.CurrentlyExecutingIds = list;
            }
        }
    }
}
