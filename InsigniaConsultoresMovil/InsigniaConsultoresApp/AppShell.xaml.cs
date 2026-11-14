using InsigniaConsultoresApp.Services.Storage;
using InsigniaConsultoresApp.Views;

namespace InsigniaConsultoresApp
{
    /// <summary>
    /// Shell raíz: ruteo de arranque (login vs Home), flyout con la sesión y cierre de sesión.
    /// Recibe <see cref="ITokenStore"/> por constructor (resuelto por DI en <c>App.CreateWindow</c>).
    /// </summary>
    public partial class AppShell : Shell
    {
        private readonly ITokenStore _tokenStore;

        public AppShell(ITokenStore tokenStore)
        {
            InitializeComponent();
            _tokenStore = tokenStore;

            // Rutas globales de las vistas de campañas (push relativo desde la lista).
            // Un solo segmento por ruta para evitar ambigüedad con la ruta "campaigns" del flyout.
            Routing.RegisterRoute("campaign-detail", typeof(CampaignDetailPage));
            Routing.RegisterRoute("campaign-recipients", typeof(RecipientsPage));

            Loaded += OnLoaded;
            Navigated += OnNavigated;
        }

        /// <summary>
        /// Decide el destino inicial: Home si hay sesión válida; si no, permanece en login
        /// (el primer <c>ShellContent</c> es login y es el contenido inicial por defecto).
        /// </summary>
        private async void OnLoaded(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;

            try
            {
                if (await _tokenStore.HasValidSessionAsync())
                    await GoToAsync("//home");
                // Sin sesión válida: queda en //login, sin flyout (configurado en XAML).
            }
            catch (Exception)
            {
                // Fallo al leer la sesión: quedarse en login (comportamiento seguro).
            }
        }

        /// <summary>
        /// Carga el nombre de usuario del flyout al entrar a cualquier pantalla autenticada
        /// (Home, Campañas y sus vistas) y lo limpia solo al volver a login.
        /// </summary>
        private void OnNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            var location = e.Current?.Location?.OriginalString ?? "";

            if (location.Contains("login", StringComparison.Ordinal))
            {
                UsernameLabel.Text = "";
                return;
            }

            if (string.IsNullOrEmpty(UsernameLabel.Text))
                _ = UpdateUsernameAsync();
        }

        /// <summary>
        /// Cierra la sesión (SecureStorage + SQLite) y regresa al login con navegación
        /// absoluta, de modo que login no queda en el back stack.
        /// Mismo estado final que <c>HomeViewModel.LogoutCommand</c>.
        /// </summary>
        private async void OnLogoutClicked(object? sender, EventArgs e)
        {
            await _tokenStore.ClearAsync();
            await GoToAsync("//login");
        }

        private async Task UpdateUsernameAsync()
        {
            try
            {
                var session = await _tokenStore.GetSessionAsync();
                UsernameLabel.Text = session?.Username ?? "";
            }
            catch (Exception)
            {
                UsernameLabel.Text = "";
            }
        }
    }
}
