using SaludTotal.Commands;
using SaludTotal.Desktop.Services;
using SaludTotal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaludTotal.ViewModels
{
    public class RequestsViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<SolicitudReprogramacion> _requests;
        private bool _isLoading = true;
        private Dictionary<string, string> _searchOptions = new()
        {
            { "Paciente", "Turno.Paciente.NombreApellido" },
            { "Profesional", "Turno.Profesional.Nombre" },
            { "Fecha", "Fecha" },
            { "Fecha original", "Turno.Fecha" }
        };
        private string _selectedSearchOption;
        //private Dictionary<string, int?> _specialtiesOptions = new();
        //private int? _selectedSpecialtyId = null;
        private string _selectedStatus;
        private string _currentSortColumn = "ID";
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
        private string _searchTerm = string.Empty;

        public RequestsViewModel(ApiService apiService)
        {
            _apiService = apiService;

            ErrorMessageviewModel = new MessageViewModel();
            StatusMessageviewModel = new MessageViewModel();

            SortCommand = new RelayCommand(_ => ExecuteSort((string)_), _ => !IsLoading, this, nameof(IsLoading));
            SearchCommand = new RelayCommand(_ => ExecuteSearch(), _ => !IsLoading, this, nameof(IsLoading));
            ClearSearchTermCommand = new RelayCommand(_ => ExecuteClearSearchTerm(), _ => true);
            ReloadCommand = new RelayCommand(_ => ExecuteReload(), _ => !IsLoading, this, nameof(IsLoading));
            //AcceptRequestCommand = new RelayCommand(ExecuteAcceptRequest, CanExecuteAcceptRequest, this, nameof(Requests), nameof(isAcceptingRequest));
            AcceptRequestCommand = new ApproveRequestAsyncCommand(this, apiService);
            RejectRequestCommand = new RejectRequestAsyncCommand(this, apiService);
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            //await LoadSpecialtyOptions();
            SelectedSearchOption = SearchOptions.First().Value;
            SelectedRequestStatus = StatusOptions.First();
            //SelectedSpecialtyId = SpecialtiesOptions.First().Value;
            await LoadRequests();
            IsLoading = false;
            StatusMessage = "Solicitudes cargadas correctamente.";
        }

        //private async Task LoadSpecialtyOptions()
        //{
        //    try
        //    {
        //        var specializationsDictionary = new Dictionary<string, int?>()
        //        {
        //            {"Todas", null }
        //        };
        //        var specializations = await _apiService.GetEspecialidadesAsync();

        //        foreach (var specialization in specializations)
        //        {
        //            specializationsDictionary.Add(specialization.Nombre, specialization.EspecialidadId);
        //        }
        //        SpecialtiesOptions = specializationsDictionary;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle error
        //        ErrorMessage = $"Error al cargar las especialidades: {ex.Message}";
        //    }
        //}

        private async Task LoadRequests()
        {
            try
            {
                //var specialtyFilter = SpecialtiesOptions.Keys.FirstOrDefault(k => SpecialtiesOptions[k] == SelectedSpecialtyId);
                //var speciality = specialtyFilter == "Todas" ? null : specialtyFilter;
                //var doctor = string.Empty;
                //var patient = string.Empty;
                //var date = string.Empty;

                //switch (SelectedSearchOption)
                //{
                //    case "Turno.Paciente.NombreCompleto":
                //        patient = SearchTerm;
                //        break;
                //    case "Turno.Profesional.Nombre":
                //        doctor = SearchTerm;
                //        break;
                //    case "Fecha":
                //        date = SearchTerm;
                //        break;
                //    case "Turno.Fecha":
                //        date = SearchTerm;
                //        break;
                //}

                var status = SelectedRequestStatus == "Todos" ? null : SelectedRequestStatus.ToLower();
                var requests = await _apiService.GetSolicitudesDeReprogramacion();
                Requests = new ObservableCollection<SolicitudReprogramacion>(requests.Solicitudes);
            }
            catch (Exception ex)
            {
                // Handle error
                ErrorMessage = $"Error al cargar las solicitudes: {ex.Message}";
            }
        }

        public MessageViewModel ErrorMessageviewModel { get; set; }
        public MessageViewModel StatusMessageviewModel { get; set; }
        public string ErrorMessage
        {
            set => ErrorMessageviewModel.Message = value;
        }

        public string StatusMessage
        {
            set => StatusMessageviewModel.Message = value;
        }

        private async void ApplyFilters()
        {
            if (IsLoading) return;

            IsLoading = true;
            await LoadRequests();
            IsLoading = false;
        }

        public ObservableCollection<SolicitudReprogramacion> Requests
        {
            get => _requests;
            set
            {
                _requests = value;
                OnPropertyChanged(nameof(Requests));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, string> SearchOptions
        {
            get => _searchOptions;
            set
            {
                _searchOptions = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSearchOption
        {
            get => _selectedSearchOption;
            set
            {
                _selectedSearchOption = value;
                OnPropertyChanged();
            }
        }

        //public Dictionary<string, int?> SpecialtiesOptions
        //{
        //    get => _specialtiesOptions;
        //    set
        //    {
        //        _specialtiesOptions = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public int? SelectedSpecialtyId
        //{
        //    get => _selectedSpecialtyId;
        //    set
        //    {
        //        _selectedSpecialtyId = value;
        //        OnPropertyChanged();
        //        ApplyFilters();
        //    }
        //}

        public List<string> StatusOptions { get; } = new List<string> {"Todos", "Pendiente", "Aceptado", "Rechazado" };

        public string SelectedRequestStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string CurrentSortColumn
        {
            get => _currentSortColumn;
            set
            {
                _currentSortColumn = value;
                OnPropertyChanged();
            }
        }

        public ListSortDirection CurrentSortDirection
        {
            get => _currentSortDirection;
            set
            {
                _currentSortDirection = value;
                OnPropertyChanged();
            }
        }

        public ICommand SortCommand { get; }

        private void ExecuteSort(string columnName)
        {
            if (CurrentSortColumn == columnName)
            {
                CurrentSortDirection = CurrentSortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                CurrentSortColumn = columnName;
                CurrentSortDirection = ListSortDirection.Ascending;
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand ClearSearchTermCommand { get; }

        private void ExecuteSearch()
        {
            ApplyFilters();
        }

        private void ExecuteClearSearchTerm()
        {
            SearchTerm = string.Empty;
            SelectedSearchOption = SearchOptions.First().Value;
        }

        public ICommand ReloadCommand { get; }

        public async void ExecuteReload()
        {
            IsLoading = true;
            await LoadRequests();
            IsLoading = false;
        }

        public ICommand AcceptRequestCommand { get; }

        public bool CanExecuteAcceptRequest(object obj)
        {
            if (IsLoading)
            {
                return false;
            }

            if (obj == null)
            {
                return false;
            }

            if (CurrentlyExecutingIds.Contains((int)obj))
            {
                return false;
            }

            var request = Requests.Where(r => r.Id == (int)obj).FirstOrDefault();

            if (request == null) { 
                return false;
            }

            if(request.Estado != EstadoSolicitud.pendiente)
            {
                return false;
            }

            return true;
        }

        public ICommand RejectRequestCommand { get; }

        public bool CanExecuteRejectRequest(object obj)
        {
            if (IsLoading)
            {
                return false;
            }

            if (obj == null)
            {
                return false;
            }

            if(CurrentlyExecutingIds.Contains((int)obj))
            {
                return false;
            }

            var request = Requests.Where(r => r.Id == (int)obj).FirstOrDefault();

            if (request == null)
            {
                return false;
            }

            if (request.Estado != EstadoSolicitud.pendiente)
            {
                return false;
            }

            return true;
        }
        private List<int> _currentlyExecutingIds = new List<int>();
        public List<int> CurrentlyExecutingIds
        {
            get { return _currentlyExecutingIds; }
            set
            {
                _currentlyExecutingIds = value;
                OnPropertyChanged();
                (RejectRequestCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();
                (AcceptRequestCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();
            }
        }
    }
}
