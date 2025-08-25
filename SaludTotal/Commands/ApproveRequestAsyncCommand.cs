using SaludTotal.Desktop.Services;
using SaludTotal.ViewModels;

namespace SaludTotal.Commands
{
    public class ApproveRequestAsyncCommand : AsyncCommandBase
    {
        private readonly RequestsViewModel _viewModel;
        private readonly ApiService _apiService;

        public ApproveRequestAsyncCommand(RequestsViewModel viewModel, ApiService apiservice)
            : base(viewModel.CanExecuteAcceptRequest!)
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
            list.Add((int) parameter!);
            _viewModel.CurrentlyExecutingIds = list;
            try
            {
                await Task.Delay(1000);
                var result = await _apiService.AceptarSolicitudReprogramacionAsync((int)parameter!);

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
