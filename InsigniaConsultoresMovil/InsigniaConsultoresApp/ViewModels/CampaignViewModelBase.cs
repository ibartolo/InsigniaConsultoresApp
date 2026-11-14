using System;
using System.Threading.Tasks;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Storage;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Base para los ViewModels de campañas: estado de carga/error/vacío y manejo
    /// centralizado del 401 (CW-R8), que limpia la sesión y redirige a //login una sola vez.
    /// </summary>
    public abstract class CampaignViewModelBase : ViewModelBase
    {
        /// <summary>Evita navegaciones duplicadas a //login cuando varios requests fallan con 401.</summary>
        private static bool _redirectingToLogin;

        private readonly ITokenStore _tokenStore;

        private bool _isBusy;
        private string? _errorMessage;
        private bool _isEmpty;

        protected CampaignViewModelBase(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        /// <summary>Indica si hay una operación en vuelo (controla spinner y botones).</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        /// <summary>Inverso de <see cref="IsBusy"/> para habilitar/deshabilitar la UI.</summary>
        public bool IsNotBusy => !IsBusy;

        public string? ErrorMessage
        {
            get => _errorMessage;
            protected set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>True cuando hay un mensaje de error que mostrar.</summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>True cuando la lista actual está vacía (sin datos o sin resultados de filtro).</summary>
        public bool IsEmpty
        {
            get => _isEmpty;
            protected set => SetProperty(ref _isEmpty, value);
        }

        /// <summary>
        /// Ejecuta <paramref name="action"/> dentro del ciclo IsBusy/ErrorMessage:
        /// limpia el error, activa IsBusy, captura excepciones inesperadas y restaura IsBusy.
        /// </summary>
        protected async Task RunAsync(Func<Task> action)
        {
            // Anti doble carga: ignora la invocación si ya hay una operación en vuelo.
            if (IsBusy)
                return;

            ErrorMessage = null;
            IsBusy = true;

            try
            {
                await action();
            }
            catch (Exception)
            {
                ErrorMessage = "Ocurrió un error";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Manejo central del 401 (CW-R8): si el resultado es <see cref="CampaignErrorKind.Unauthorized"/>,
        /// limpia la sesión y navega a //login exactamente una vez (guard anti duplicados).
        /// </summary>
        protected async Task HandleUnauthorizedAsync(CampaignErrorKind kind)
        {
            if (kind != CampaignErrorKind.Unauthorized || _redirectingToLogin)
                return;

            _redirectingToLogin = true;
            try
            {
                await _tokenStore.ClearAsync();
                await Shell.Current.GoToAsync("//login");
            }
            finally
            {
                _redirectingToLogin = false;
            }
        }
    }
}
