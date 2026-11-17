using System.Net.Http;
using InsigniaConsultoresApp.Services.Api;
using InsigniaConsultoresApp.Services.Storage;
using InsigniaConsultoresApp.ViewModels;
using InsigniaConsultoresApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InsigniaConsultoresApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Android: quita la línea nativa del Entry (equivale a android:background="@null").
            // Se hace con SetBackgroundColor para no depender del tema MaterialComponents.
#if ANDROID
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
                "NoUnderline",
                (handler, view) => handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent));
#endif

            // HttpClient único del API: BaseAddress y Timeout centralizados en AuthConfig.
            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri(AuthConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(AuthConfig.RequestTimeoutSeconds),
            });

            // Servicios de dominio (singleton): sin estado o con estado compartido de sesión.
            builder.Services.AddSingleton<SessionDatabase>();
            builder.Services.AddSingleton<ITokenStore, TokenStore>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ICampaignService, CampaignService>();
            builder.Services.AddSingleton<IReportsService, ReportsService>();

            // ViewModels (transient): estado por vista/vida.
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<CampaignsViewModel>();
            builder.Services.AddTransient<CampaignDetailViewModel>();
            builder.Services.AddTransient<RecipientsViewModel>();

            // Páginas y Shell (transient): Shell resuelve los DataTemplate con inyección por constructor.
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CampaignsPage>();
            builder.Services.AddTransient<CampaignDetailPage>();
            builder.Services.AddTransient<RecipientsPage>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
