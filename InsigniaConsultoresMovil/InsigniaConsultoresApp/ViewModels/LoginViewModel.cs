using System;
using System.Windows.Input;
using InsigniaConsultoresApp.Models.Requests;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Api;
using InsigniaConsultoresApp.Services.Storage;
using InsigniaConsultoresApp.Validations;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Estado del formulario de login. Orquesta validación → autenticación → persistencia → navegación.
    /// </summary>
    public sealed class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenStore _tokenStore;

        private string _username = "";
        private string _password = "";
        private bool _isBusy;
        private string _errorMessage = "";

        public LoginViewModel(IAuthService authService, ITokenStore tokenStore)
        {
            _authService = authService;
            _tokenStore = tokenStore;

            LoginCommand = new Command(async () => await LoginAsync());
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>Indica si hay una solicitud de login en vuelo (controla el spinner y el botón).</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        /// <summary>Inverso de <see cref="IsBusy"/> para habilitar/deshabilitar el botón.</summary>
        public bool IsNotBusy => !IsBusy;

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>True cuando hay un mensaje de error que mostrar.</summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            // Anti doble envío: si ya hay una solicitud en vuelo, ignorar el toque.
            if (IsBusy)
                return;

            // Validación síncrona antes de enviar la solicitud.
            var validation = LoginValidator.Validate(Username, Password);
            if (!validation.IsValid)
            {
                ErrorMessage = string.Join(Environment.NewLine, validation.Messages);
                return;
            }

            ErrorMessage = "";
            IsBusy = true;

            try
            {
                var result = await _authService.LoginAsync(new TokenRequest
                {
                    Username = Username,
                    Password = Password,
                });

                if (!result.Success || result.Session is null)
                {
                    ErrorMessage = result.Message;
                    return;
                }

                await _tokenStore.SaveSessionAsync(result.Session);

                // Limpia la contraseña de memoria tras un login exitoso.
                Password = "";

                // Navegación absoluta: reemplaza el stack para que el back no regrese al login.
                await Shell.Current.GoToAsync("//home");
            }
            catch (Exception)
            {
                // Errores inesperados (p. ej. ruta de navegación aún no registrada).
                ErrorMessage = "Ocurrió un error";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
