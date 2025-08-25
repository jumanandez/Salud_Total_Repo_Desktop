using SaludTotal.Commands;
using SaludTotal.Desktop;
using SaludTotal.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SaludTotal.ViewModels
{
    public class AddProfessionalViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly ErrorsViewModel _errorsViewModel;
        private readonly Profesional _profesionalModel;
        private readonly ProfessionalManager _professionalManager;

        private string _name = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private int? _specialtyId = null;
        public MessageViewModel ErrorMessageviewModel { get; set; } = new();
        public MessageViewModel StatusMessageviewModel { get; set; } = new();

        public AddProfessionalViewModel(Profesional? profesional = null)
        {
            _errorsViewModel = new ErrorsViewModel();
            _professionalManager = new ProfessionalManager();
            _profesionalModel = profesional ?? new Profesional();
            CancelCommand = new RelayCommand(Cancel, CanCancel);
            CreateCommand = new CreateProfessionalCommand(this);
            SetProfessionalValues();
            _errorsViewModel.ErrorsChanged += ErrorsViewModel_ErrorsChanged;
        }

        public async Task InitializeAsync()
        {
            await SetSpecializations();
        }

        private void SetProfessionalValues()
        {
            _name = _profesionalModel.NombreApellido;
            _lastName = _profesionalModel.NombreCompleto;
            _email = _profesionalModel.Email;
            _phone = _profesionalModel.Telefono;
            _specialtyId = _profesionalModel.EspecialidadId;
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public event Action? RequestClose;

        private void ErrorsViewModel_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
            OnPropertyChanged(nameof(CanCreate));
        }

        public ICommand CancelCommand { get; set; }

        private bool CanCancel(object obj)
        {
            return true;
        }

        private void Cancel(object obj)
        {
            RequestClose?.Invoke();
        }

        public ICommand CreateCommand { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                ClearErrors();

                if (string.IsNullOrEmpty(value))
                {
                    AddError("El nombre no puede estar vacío.");
                }

                if (value.Length < 3)
                {
                    AddError("El nombre debe tener al menos 3 caracteres.");
                }

                OnPropertyChanged();
                (CreateCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                ClearErrors();

                if (string.IsNullOrEmpty(value))
                {
                    AddError("El apellido no puede estar vacío.");
                }

                if (value.Length < 3)
                {
                    AddError("El apellido debe tener al menos 3 caracteres.");
                }

                OnPropertyChanged();
                (CreateCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();

            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                ClearErrors();
                if (string.IsNullOrWhiteSpace(value))
                {
                    AddError("El email no puede estar vacío.");
                }
                if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    AddError("El email no es válido.");
                }
                OnPropertyChanged();
                (CreateCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                ClearErrors();
                if (string.IsNullOrWhiteSpace(value))
                {
                    AddError("El teléfono no puede estar vacío.");
                }
                if (!Regex.IsMatch(value, @"^\d{10}$"))
                {
                    AddError("El teléfono debe tener 10 dígitos.");
                }
                OnPropertyChanged();
                (CreateCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();
            }
        }

        public Dictionary<string, int> Specialties
        {
            get; set;
        } = new();

        private async Task SetSpecializations()
        {
            var data = await _professionalManager.GetSpecializations();
            Specialties = data.ToDictionary(e => e.Nombre, e => e.EspecialidadId);
            OnPropertyChanged(nameof(Specialties));
        }

        public int? SpecialtyId
        {
            get => _specialtyId;
            set
            {
                _specialtyId = value;
                ClearErrors();
                if (value <= 0)
                {
                    AddError("Debe seleccionar una especialidad válida.");
                }
                OnPropertyChanged();
                (CreateCommand as AsyncCommandBase)?.RaiseCanExecuteChanged();
            }
        }


        public bool HasErrors => _errorsViewModel.HasErrors;

        public bool CanCreate =>
            !HasErrors &&
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Phone) &&
            SpecialtyId != null;

        public string ErrorMessage
        {
            set => ErrorMessageviewModel.Message = value;
        }

        public string StatusMessage
        {
            set => StatusMessageviewModel.Message = value;
        }

        public IEnumerable GetErrors([CallerMemberName] string? propertyName = null)
        {
            return _errorsViewModel.GetErrors(propertyName);
        }

        public void AddError(string errorMessage, [CallerMemberName] string? propertyName = null)
        {
            _errorsViewModel.AddError(errorMessage, propertyName);
        }

        public void ClearErrors([CallerMemberName] string? propertyName = null)
        {
            _errorsViewModel.ClearErrors(propertyName);
        }
    }
}
