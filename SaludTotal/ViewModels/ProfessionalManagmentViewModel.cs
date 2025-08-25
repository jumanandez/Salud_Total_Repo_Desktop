using SaludTotal.Desktop.Services;
using SaludTotal.Models;
using SaludTotal.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using SaludTotal.Commands;
using SaludTotal.Desktop.Views;
using SaludTotal.Views;
using System.Windows;

public class ProfessionalManagmentViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly ProfessionalManager _professionalManager = new ProfessionalManager();
    private ObservableCollection<Profesional> _professionals;
    private string _currentSortColumn = "ID";
    private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
    private bool _isLoading;

    public event Action? RequestClose;
    public Window? Window;

    public ObservableCollection<Profesional> Professionals
    {
        get => _professionals;
        set
        {
            _professionals = value;
            OnPropertyChanged(nameof(Professionals));
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

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    private string _searchTerm = string.Empty;

    public string SearchTerm { 
        get => _searchTerm;
        set {
            _searchTerm = value;
            OnPropertyChanged();}
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

    public ICommand SortCommand { get; }

    public ICommand SearchCommand { get; }

    public ICommand ClearSearchTermCommand { get; }

    public ProfessionalManagmentViewModel()
    {
        ErrorMessageviewModel = new MessageViewModel();
        StatusMessageviewModel = new MessageViewModel();
        Professionals = new ObservableCollection<Profesional>();

        SortCommand = new RelayCommand(_ => ExecuteSort((string)_), _=> true);
        SearchCommand = new RelayCommand(_ => ExecuteSearch(), _ => !IsLoading, this, nameof(IsLoading));
        ClearSearchTermCommand = new RelayCommand(_ => ExecuteClearSearchTerm(), _ => !IsLoading, this, nameof(IsLoading));

        SetSpecialityCommand = new RelayCommand(_ => ExecuteSetSpecialization((int?)_), _ => !IsLoading, this, nameof(IsLoading));
        ClearSpecialityFilterCommand = new RelayCommand(_ => ExecuteClearSpecialityFilter(), _ => !IsLoading, this, nameof(IsLoading));

        ManageProfessionalCommand = new RelayCommand(professionalId => ExecuteManageProfessional((int)professionalId), _ => !IsLoading);

        ReloadCommand = new RelayCommand(_ => ExecuteReload(), _ => !IsLoading, this, nameof(IsLoading));

        AddProfessionalCommand = new RelayCommand(_ => ExecuteAddProfessional(), _ => !IsLoading, this, nameof(IsLoading));
    }

    private async void ExecuteSort(string columnName)
    {
        if (IsLoading) return;

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

        await CargarDoctoresAsync();

        var sortedList = CurrentSortDirection == ListSortDirection.Ascending
            ? _professionals.OrderBy(p => p.GetType().GetProperty(CurrentSortColumn)?.GetValue(p, null)).ToList()
            : _professionals.OrderByDescending(p => p.GetType().GetProperty(CurrentSortColumn)?.GetValue(p, null)).ToList();

        Professionals = new ObservableCollection<Profesional>(sortedList);
    }

    private async void ExecuteSearch()
    {
        if(IsLoading) return;
        await CargarDoctoresAsync();
        if (!string.IsNullOrEmpty(SearchTerm))
        {
            var filtered = _professionals.Where(p => p.NombreApellido.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            Professionals = new ObservableCollection<Profesional>(filtered);
        }
    }

    private async void ExecuteClearSearchTerm()
    {
        try
        {
            SearchTerm = string.Empty;
            await CargarDoctoresAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al limpiar el término de búsqueda: {ex.Message}";
        }
    }

    private async Task CargarDoctoresAsync()
    {
        try
        {
            IsLoading = true;

            await Task.Delay(1000);

            var professionalsList = await _professionalManager.getProfessionals();

            Professionals.Clear();
            foreach (var prof in professionalsList)
            {
                Professionals.Add(prof);
            }
            StatusMessage = "Doctores cargados correctamente.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar los doctores: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ICommand SetSpecialityCommand { get; }

    private async void ExecuteSetSpecialization(int? specializationId)
    {
        if (IsLoading) return;
        SelectedSpecialtyId = specializationId;
        await CargarDoctoresAsync();
        if (specializationId != null)
        {
            var filtered = _professionals
                .Where(p => p.EspecialidadId == SelectedSpecialtyId)
                .ToList();
            Professionals = new ObservableCollection<Profesional>(filtered);
        }
    }

    public ICommand ClearSpecialityFilterCommand { get; }

    private async void ExecuteClearSpecialityFilter()
    {
        SelectedSpecialtyId = null;
        await CargarDoctoresAsync();
    }

    public ObservableCollection<Especialidad> Specialties
    { get; set; } = new ObservableCollection<Especialidad>();

    private int? _selectedSpecialtyId;

    public int? SelectedSpecialtyId
    {
        get => _selectedSpecialtyId;
        set
        {
            _selectedSpecialtyId = value;
            OnPropertyChanged(nameof(SelectedSpecialtyId));
        }
    }

    public ICommand ManageProfessionalCommand { get; }

    private void ExecuteManageProfessional(int professionalId)
    {
        try
        {
            var professional = _professionals.FirstOrDefault(p => p.Id == professionalId);
            if(professional != null)
            {
                var detalleProfesionalWindow = new DetalleProfesionalWindow(professional);
                detalleProfesionalWindow.Show();
                RequestClose?.Invoke();
            }
            else
            {
                ErrorMessage = "Profesional no encontrado.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al obtener datos del profesinal: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ICommand ReloadCommand { get; }

    private async void ExecuteReload()
    {
        await CargarDoctoresAsync();
        SearchTerm = string.Empty;
        SelectedSpecialtyId = null;
        CurrentSortColumn = "ID";
        CurrentSortDirection = ListSortDirection.Ascending;
    }

    public ICommand AddProfessionalCommand { get; }

    private void ExecuteAddProfessional()
    {
        try
        {
            var addProfesionalWindow = new AddProfessional();
            addProfesionalWindow.Owner = Window;
            addProfesionalWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addProfesionalWindow.Show();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al abrir la ventana de agregar profesional: {ex.Message}";
        }
    }

    private async Task SetSpecializations()
    {
        try
        {
            var data = await _professionalManager.GetSpecializations();
            Specialties = new ObservableCollection<Especialidad>(data);
            OnPropertyChanged(nameof(Specialties));
        }
        catch (Exception ex) { 
            ErrorMessage = $"Error al cargar las especialidades: {ex.Message}";
        }
    }

    public async Task InitializeAsync()
    {
        await CargarDoctoresAsync();
        await SetSpecializations();
    }
}

public class SortInfo
{
    public string Column { get; set; }
    public ListSortDirection Direction { get; set; }
}