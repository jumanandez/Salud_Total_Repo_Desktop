// SaludTotal.Desktop/ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaludTotal.Desktop.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SaludTotal.Desktop.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isLoading;

        // Propiedades para comunicar el resultado a la vista
        public bool LoginExitoso { get; private set; } = false;
        public Action CloseAction { get; set; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
        }

        // Usamos [RelayCommand] para el botón de Login
        [RelayCommand]
        private async Task Login(PasswordBox passwordBox)
        {
            if (passwordBox == null || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                ErrorMessage = "Por favor, ingrese la clave de acceso.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            bool success = await _apiService.LoginAsync(passwordBox.Password);

            IsLoading = false;

            if (success)
            {
                LoginExitoso = true;
                CloseAction?.Invoke(); // Cierra la ventana de login
            }
            else
            {
                ErrorMessage = "Clave de acceso incorrecta.";
            }
        }
    }
}