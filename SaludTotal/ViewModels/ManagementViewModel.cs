using SaludTotal.Commands;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SaludTotal.ViewModels
{
    public class ManagementViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private bool _isLoading;

        public event Action? RequestClose;

        public ManagementViewModel()
        {
            _apiService = new ApiService();

            ShiftsViewModel = new ShiftsViewModel(_apiService);
            ShiftsViewModel.RequestClose += () => RequestClose?.Invoke();

            RequestsViewModel = new RequestsViewModel(_apiService);

        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            var shiftsTask = ShiftsViewModel.InitializeAsync();
            var requestsTask = RequestsViewModel.InitializeAsync();

            await Task.WhenAll(shiftsTask, requestsTask);

            IsLoading = false;
        }

        //public MessageViewModel ErrorMessageviewModel { get; set; }
        //public MessageViewModel StatusMessageviewModel { get; set; }

        //public string ErrorMessage
        //{
        //    set => ErrorMessageviewModel.Message = value;
        //}

        //public string StatusMessage
        //{
        //    set => StatusMessageviewModel.Message = value;
        //}

        public ShiftsViewModel ShiftsViewModel { get; }
        public RequestsViewModel RequestsViewModel { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }
}
