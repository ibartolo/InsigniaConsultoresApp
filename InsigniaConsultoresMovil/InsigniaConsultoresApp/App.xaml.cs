using Microsoft.Extensions.DependencyInjection;

namespace InsigniaConsultoresApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Resolver AppShell desde el contenedor DI para que reciba ITokenStore por constructor.
            // IPlatformApplication.Current ya está establecido cuando se llama a CreateWindow.
            return new Window(IPlatformApplication.Current!.Services.GetRequiredService<AppShell>());
        }
    }
}