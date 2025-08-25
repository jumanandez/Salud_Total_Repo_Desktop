using SaludTotal.Commands;
using SaludTotal.Desktop.Services;
using SaludTotal.Desktop.Views;
using SaludTotal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SaludTotal.ViewModels
{
    public class ShiftsViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Turno> _shifts;
        private bool _isLoading = true;
        private Dictionary<string, string> _searchOptions = new()
        {
            { "Paciente", "Paciente.NombreCompleto" },
            { "Profesional", "Profesional.Nombre" },
            { "Fecha", "Fecha" }
        };
        private string _selectedSearchOption;
        private Dictionary<string, int?> _specialtiesOptions = new();
        private int? _selectedSpecialtyId = null;
        private string _selectedStatus;
        private string _currentSortColumn = "ID";
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
        private string _searchTerm = string.Empty;

        public event Action? RequestClose;

        public ShiftsViewModel(ApiService apiService)
        {
            _apiService = apiService;;

            ErrorMessageviewModel = new MessageViewModel();
            StatusMessageviewModel = new MessageViewModel();
            SortCommand = new RelayCommand(_ => ExecuteSort((string)_), _ => !IsLoading, this, nameof(IsLoading));
            SearchCommand = new RelayCommand(_ => ExecuteSearch(), _ => !IsLoading, this, nameof(IsLoading));
            ClearSearchTermCommand = new RelayCommand(_ => ExecuteClearSearchTerm(), _ => true);
            ReloadCommand = new RelayCommand(_ => ExecuteReload(), _ => !IsLoading, this, nameof(IsLoading));
            AddShiftCommand = new RelayCommand(_ => ExecuteAddShift(), _ => !IsLoading, this, nameof(IsLoading));
            ManageShiftCommand = new RelayCommand(_ => ExecuteManageShift((int)_), _=> !IsLoading, this, nameof(IsLoading));
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;
            await LoadSpecialtyOptions();
            SelectedSearchOption = SearchOptions.First().Value;
            SelectedShiftStatus = StatusOptions.First();
            if (SpecialtiesOptions.Any())
            {
                SelectedSpecialtyId = SpecialtiesOptions.First().Value;
            }
            await LoadShifts();
            IsLoading = false;
        }

        private async Task LoadSpecialtyOptions()
        {
            try
            {
                var specializationsDictionary = new Dictionary<string, int?>()
                {
                    {"Todas", null }
                };
                var specializations = await _apiService.GetEspecialidadesAsync();

                foreach (var specialization in specializations)
                {
                    specializationsDictionary.Add(specialization.Nombre, specialization.EspecialidadId);
                }
                SpecialtiesOptions = specializationsDictionary;
            }
            catch (Exception ex)
            {
                // Handle error
                ErrorMessage = $"Error al cargar las especialidades: {ex.Message}";
            }
        }

        private async Task LoadShifts()
        {
            try
            {
                var specialtyFilter = SpecialtiesOptions.Keys.FirstOrDefault(k => SpecialtiesOptions[k] == SelectedSpecialtyId);
                var speciality = specialtyFilter == "Todas" ? null : specialtyFilter;
                var doctor = string.Empty;
                var patient = string.Empty;
                var date = string.Empty;

                switch (SelectedSearchOption)
                {
                    case "Paciente.NombreCompleto":
                        patient = SearchTerm;
                        break;
                    case "Profesional.Nombre":
                        doctor = SearchTerm;
                        break;
                    case "Fecha":
                        date = SearchTerm;
                        break;
                }

                var status = SelectedShiftStatus == "Todos" ? null : SelectedShiftStatus;
                var shifts = await _apiService.GetTurnosAsync(speciality, date, doctor, patient, status);
                Shifts = new ObservableCollection<Turno>(shifts);
            }
            catch (Exception ex)
            {
                // Handle error
                ErrorMessage = ($"Error al cargar los turnos: {ex.Message}");
            }
        }

        private async void ApplyFilters()
        {
            if (IsLoading) return;
            IsLoading = true;
            await LoadShifts();
            IsLoading = false;
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

        public ObservableCollection<Turno> Shifts
        {
            get => _shifts;
            set
            {
                _shifts = value;
                OnPropertyChanged(nameof(Shifts));
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

        public Dictionary<string, int?> SpecialtiesOptions
        {
            get => _specialtiesOptions;
            set
            {
                _specialtiesOptions = value;
                OnPropertyChanged();
            }
        }

        public int? SelectedSpecialtyId
        {
            get => _selectedSpecialtyId;
            set
            {
                _selectedSpecialtyId = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public List<string> StatusOptions { get; } = Enum.GetNames(typeof(EstadoTurno)).ToList();

        public string SelectedShiftStatus
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
            SelectedSearchOption = "Paciente.NombreCompleto";
        }

        public ICommand ReloadCommand { get; }

        private async void ExecuteReload()
        {
            IsLoading = true;
            await LoadShifts();
            IsLoading = false;
        }

        public ICommand AddShiftCommand { get; }

        private void ExecuteAddShift()
        {
            try
            {
                var nuevoTurnoWindow = new NuevoTurnoWindow();
                nuevoTurnoWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                nuevoTurnoWindow.Show();
                this.RequestClose?.Invoke();
            }
            catch (Exception ex) {
                ErrorMessage = $"Error al abrir la ventana de agregar profesional: {ex.Message}";
            }
        }

        public ICommand ManageShiftCommand { get; }

        private void ExecuteManageShift(int id)
        {
            try
            {
                var shift = Shifts.First(s => s.Id == id);

                if (shift == null)
                {
                    ErrorMessage = $"Error al obtener datos del turno";
                }

                var nuevoTurnoWindow = new DetalleTurnoWindow(shift!);
                nuevoTurnoWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                nuevoTurnoWindow.Show();
                this.RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al mostrar detalles del turno: {ex.Message}";
            }
        }
    }
}
