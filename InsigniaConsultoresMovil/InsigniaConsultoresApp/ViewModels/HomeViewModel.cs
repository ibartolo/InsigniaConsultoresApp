using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaConsultoresApp.Services.Storage;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Estado de la pantalla Home (post-autenticación). Expone el usuario de la sesión y el cierre de sesión.
    /// </summary>
    public sealed class HomeViewModel : ViewModelBase
    {
        private readonly ITokenStore _tokenStore;

        private string _username = "";

        public HomeViewModel(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;

            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        /// <summary>Nombre de usuario autenticado (para el header del flyout).</summary>
        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Carga el nombre de usuario desde la sesión persistida.
        /// Se invoca desde el code-behind al aparecer la página.
        /// </summary>
        public async Task LoadAsync()
        {
            var session = await _tokenStore.GetSessionAsync();
            Username = session?.Username ?? "";
        }

        private async Task LogoutAsync()
        {
            await _tokenStore.ClearAsync();

            // Navegación absoluta a login: limpia el stack y el flyout.
            await Shell.Current.GoToAsync("//login");
        }
    }
}
